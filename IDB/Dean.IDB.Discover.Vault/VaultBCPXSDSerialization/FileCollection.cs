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
    /// This class is a collection of the Property instances.   
    /// </summary>
    public class FileCollection : CollectionBase, IEnumerable<File>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(File p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(File p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public File this[int index]
        {
            get { return (File)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<File> GetEnumerator()
        {
            foreach (File file in this.List)
            {
                yield return file;
            }
        }

        #endregion IEnumerable Methods

    }
}
