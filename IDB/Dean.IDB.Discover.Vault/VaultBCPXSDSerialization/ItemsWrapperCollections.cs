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
    /// This class is a collection of the ItemswrapperItemsItemMaster instances.   
    /// </summary>
    public class ItemMasterCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMaster>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMaster p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMaster p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMaster this[int index]
        {
            get { return (ItemswrapperItemsItemMaster)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMaster> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMaster obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the ItemswrapperItemsItemMasterRev instances.   
    /// </summary>
    public class ItemMasterRevCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMasterRev>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMasterRev p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMasterRev p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMasterRev this[int index]
        {
            get { return (ItemswrapperItemsItemMasterRev)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMasterRev> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMasterRev obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }


    /// <summary>
    /// This class is a collection of the ItemswrapperItemsItemMasterItemRefDesMaster instances.   
    /// </summary>
    public class ItemRefDesMasterCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMasterItemRefDesMaster>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMasterItemRefDesMaster p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMasterItemRefDesMaster p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMasterItemRefDesMaster this[int index]
        {
            get { return (ItemswrapperItemsItemMasterItemRefDesMaster)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMasterItemRefDesMaster> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMasterItemRefDesMaster obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the Iteration instances.   
    /// </summary>
    public class IterationCollection : CollectionBase, IEnumerable<Iteration>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(Iteration p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(Iteration p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public Iteration this[int index]
        {
            get { return (Iteration)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<Iteration> GetEnumerator()
        {
            foreach (Iteration obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration instances.   
    /// </summary>
    public class ItemRefDesIterationCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration this[int index]
        {
            get { return (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIteration obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the IterationUDP instances.   
    /// </summary>
    public class IterationUDPCollection : CollectionBase, IEnumerable<IterationUDP>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationUDP p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationUDP p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationUDP this[int index]
        {
            get { return (IterationUDP)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationUDP> GetEnumerator()
        {
            foreach (IterationUDP obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the IterationAttachment instances.   
    /// </summary>
    public class IterationAttachmentCollection : CollectionBase, IEnumerable<IterationAttachment>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationAttachment p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationAttachment p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationAttachment this[int index]
        {
            get { return (IterationAttachment)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationAttachment> GetEnumerator()
        {
            foreach (IterationAttachment obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the IterationBomLink instances.   
    /// </summary>
    public class IterationBomLinkCollection : CollectionBase, IEnumerable<IterationBomLink>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationBomLink p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationBomLink p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationBomLink this[int index]
        {
            get { return (IterationBomLink)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationBomLink> GetEnumerator()
        {
            foreach (IterationBomLink obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the IterationDetail instances.   
    /// </summary>
    public class IterationDetailCollection : CollectionBase, IEnumerable<IterationDetail>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationDetail p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationDetail p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationDetail this[int index]
        {
            get { return (IterationDetail)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationDetail> GetEnumerator()
        {
            foreach (IterationDetail obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }
    /// <summary>
    /// This class is a collection of the IterationDesignDoc instances.   
    /// </summary>
    public class IterationDesignDocCollection : CollectionBase, IEnumerable<IterationDesignDoc>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationDesignDoc p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationDesignDoc p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationDesignDoc this[int index]
        {
            get { return (IterationDesignDoc)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationDesignDoc> GetEnumerator()
        {
            foreach (IterationDesignDoc obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the IterationEffectivity instances.   
    /// </summary>
    public class IterationEffectivityCollection : CollectionBase, IEnumerable<IterationEffectivity>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationEffectivity p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationEffectivity p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationEffectivity this[int index]
        {
            get { return (IterationEffectivity)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationEffectivity> GetEnumerator()
        {
            foreach (IterationEffectivity obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }


    /// <summary>
    /// This class is a collection of the IterationBomLinkBomLinkProperty instances.   
    /// </summary>
    public class IterationBomLinkBomLinkPropertyCollection : CollectionBase, IEnumerable<IterationBomLinkBomLinkProperty>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(IterationBomLinkBomLinkProperty p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(IterationBomLinkBomLinkProperty p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public IterationBomLinkBomLinkProperty this[int index]
        {
            get { return (IterationBomLinkBomLinkProperty)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<IterationBomLinkBomLinkProperty> GetEnumerator()
        {
            foreach (IterationBomLinkBomLinkProperty obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }


    /// <summary>
    /// This class is a collection of the ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance instances.   
    /// </summary>
    public class ItemRefDesInstanceCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance this[int index]
        {
            get { return (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationItemRefDesInstance obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }

    /// <summary>
    /// This class is a collection of the ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp instances.   
    /// </summary>
    public class ItemRefDesIterationPropCollection : CollectionBase, IEnumerable<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp>
    {
        #region CollectionBase Methods

        /// <summary>
        /// Adds a Field instance to the collection
        /// </summary>
        /// <param name="p">The instance that needs adding</param>
        public void Add(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp p)
        {
            this.List.Add(p);
        }



        /// <summary>
        /// Removes an instance from the collection
        /// </summary>
        /// <param name="p">The instance to remove</param>
        public void Remove(ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp p)
        {
            this.List.Remove(p);
        }


        /// <summary>
        /// Gets an instance based on its index in the collection
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The instance at the given index.</returns>
        public ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp this[int index]
        {
            get { return (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp)this.List[index]; }
            set { this.List[index] = value; }
        }

        #endregion CollectionBase Methods

        #region IEnumerable Methods

        public new IEnumerator<ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp> GetEnumerator()
        {
            foreach (ItemswrapperItemsItemMasterItemRefDesMasterItemRefDesIterationProp obj in this.List)
            {
                yield return obj;
            }
        }

        #endregion IEnumerable Methods

    }


}
