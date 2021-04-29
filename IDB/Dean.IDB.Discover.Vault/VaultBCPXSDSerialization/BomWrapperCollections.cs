using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace VaultBCPXSDSerialization
{
    /// <summary>
    /// This class is a collection of the BOMComponent instances.   
    /// </summary>
    public class BOMComponentCollection : CollectionBase, IEnumerable<BOMComponent>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(BOMComponent p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(BOMComponent p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public BOMComponent this[int index]
        {
            get { return (BOMComponent)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<BOMComponent> GetEnumerator()
        {
            foreach (BOMComponent obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the instance instances.   
    /// </summary>
    public class instanceCollection : CollectionBase, IEnumerable<instance>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(instance p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(instance p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public instance this[int index]
        {
            get { return (instance)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<instance> GetEnumerator()
        {
            foreach (instance obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the occurrence occurrences.   
    /// </summary>
    public class occurrenceCollection : CollectionBase, IEnumerable<occurrence>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field occurrence to the collection
        /// </summary>
        /// <param name="p">The occurrence that needs adding</param>
        public void Add(occurrence p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an occurrence from the collection
        /// </summary>
        /// <param name="p">The occurrence to remove</param>
        public void Remove(occurrence p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an occurrence based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The occurrence at the given index.</returns>
        public occurrence this[int index]
        {
            get { return (occurrence)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<occurrence> GetEnumerator()
        {
            foreach (occurrence obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the BOMComponentItemToComp BOMComponentItemToComps.   
    /// </summary>
    public class BOMComponentItemToCompCollection : CollectionBase, IEnumerable<BOMComponentItemToComp>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field BOMComponentItemToComp to the collection
        /// </summary>
        /// <param name="p">The BOMComponentItemToComp that needs adding</param>
        public void Add(BOMComponentItemToComp p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an BOMComponentItemToComp from the collection
        /// </summary>
        /// <param name="p">The BOMComponentItemToComp to remove</param>
        public void Remove(BOMComponentItemToComp p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an BOMComponentItemToComp based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The BOMComponentItemToComp at the given index.</returns>
        public BOMComponentItemToComp this[int index]
        {
            get { return (BOMComponentItemToComp)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<BOMComponentItemToComp> GetEnumerator()
        {
            foreach (BOMComponentItemToComp obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the BOMComponentProperty BOMComponentPropertys.   
    /// </summary>
    public class BOMComponentPropertyCollection : CollectionBase, IEnumerable<BOMComponentProperty>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field BOMComponentProperty to the collection
        /// </summary>
        /// <param name="p">The BOMComponentProperty that needs adding</param>
        public void Add(BOMComponentProperty p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an BOMComponentProperty from the collection
        /// </summary>
        /// <param name="p">The BOMComponentProperty to remove</param>
        public void Remove(BOMComponentProperty p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an BOMComponentProperty based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The BOMComponentProperty at the given index.</returns>
        public BOMComponentProperty this[int index]
        {
            get { return (BOMComponentProperty)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<BOMComponentProperty> GetEnumerator()
        {
            foreach (BOMComponentProperty obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }


}
