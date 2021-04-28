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
    /// This class is a collection of the FileRevision instances
    /// </summary>

    public class FileRevisionCollection : CollectionBase, IEnumerable<FileRevision>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(FileRevision p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(FileRevision p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public FileRevision this[int index]
        {
            get { return (FileRevision)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<FileRevision> GetEnumerator()
        {
            foreach (FileRevision fr in this.List)
            {
                yield return fr;
            }
        }

        #endregion IEnumerable Methods

    }
}
