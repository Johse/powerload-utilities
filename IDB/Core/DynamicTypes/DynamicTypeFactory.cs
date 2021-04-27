using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IDB.Core.Extensions;
using IDB.Core.DapperHelper;

namespace IDB.Core.DynamicTypes
{
    public class DynamicTypeFactory
    {
        private TypeBuilder _typeBuilder;

        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;

        public DynamicTypeFactory()
        {
            var uniqueIdentifier = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(uniqueIdentifier);

            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(uniqueIdentifier);
        }

        public Type CreateNewTypeWithDynamicProperties(Type parentType, string tableName, IEnumerable<DynamicProperty> dynamicProperties)
        {
            _typeBuilder = _moduleBuilder.DefineType(parentType.Name + "Ex", TypeAttributes.Public);

            var propertyInfos = parentType.GetProperties();
            foreach (var propertyInfo in propertyInfos)
                AddPropertyToType(propertyInfo);

            foreach (DynamicProperty property in dynamicProperties)
                AddDynamicPropertyToType(property);

            //var attributeType = typeof(Dapper.Contrib.Extensions.TableAttribute);
            //var attributeBuilder = new CustomAttributeBuilder(
            //    attributeType.GetConstructor(new Type[] { typeof(string) }) ?? throw new InvalidOperationException(),
            //    new object[] { tableName },
            //    new PropertyInfo[] { },
            //    new object[] { }
            //);
            //_typeBuilder.SetCustomAttribute(attributeBuilder);

            return _typeBuilder.CreateType();
        }

        private void AddPropertyToType(PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType;
            FieldBuilder fieldBuilder = _typeBuilder.DefineField(propertyName, propertyType, FieldAttributes.Private);

            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyInfo.PropertyType, Type.EmptyTypes);
            ILGenerator propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = _typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new Type[] { propertyType });
            ILGenerator propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);

            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            foreach (var customAttribute in propertyInfo.CustomAttributes)
            {
                var customAttributeBuilder = new CustomAttributeBuilder(
                    customAttribute.Constructor, 
                    customAttribute.ConstructorArguments.Select(c => c.Value).ToArray());
                propertyBuilder.SetCustomAttribute(customAttributeBuilder);
            }
        }

        private void AddDynamicPropertyToType(DynamicProperty dynamicProperty)
        {
            Type propertyType = dynamicProperty.SystemType;
            string propertyName = dynamicProperty.PropertyName.Replace(" ", "_");
            string fieldName = $"_{propertyName.ToCamelCase()}";

            FieldBuilder fieldBuilder = _typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);

            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            ILGenerator propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = _typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new Type[] {propertyType});
            ILGenerator propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);

            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            var attributeType = typeof(ColumnAttribute);
            var attributeBuilder = new CustomAttributeBuilder(
                attributeType.GetConstructor(new Type[] { typeof(string) }),
                new object[] { dynamicProperty.PropertyName },
                new PropertyInfo[] { },
                new object[] { }
            );
            propertyBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}