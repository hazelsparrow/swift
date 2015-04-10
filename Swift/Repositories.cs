using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Swift
{
    public interface IFilter
    {
        IEnumerable<DbParameter> GetParameters();
    }

    public class EmptyFilter : IFilter
    {
        public IEnumerable<DbParameter> GetParameters()
        {
            return new List<DbParameter>();
        }

        private static EmptyFilter instance = new EmptyFilter();
        public static EmptyFilter Instance
        {
            get
            {
                return instance; 
            }
        }
    }

    public interface IListRepository<T>
    {
        IEnumerable<T> GetAll(IFilter filter = null);
    }

    public interface ISingleItemRepository<T>
    {
        T GetSingle(object id);
        void DeleteSingle(object id);
    }

    /// <summary>
    /// Specifies the name of the stored proc retrieving the list of items for a given repository.
    /// </summary>
    public class GetAllAttribute : Attribute
    {
        public string StoredProcName { get; set; }

        public GetAllAttribute(string storedProcName)
        {
            StoredProcName = storedProcName;
        }
    }

    public class GetSingleAttribute : Attribute
    {
        public string StoredProcName { get; set; }

        public GetSingleAttribute(string storedProcName)
        {
            StoredProcName = storedProcName;
        }
    }

    public class DeleteSingleAttribute : Attribute
    {
        public string StoredProcName { get; set; }

        public DeleteSingleAttribute(string storedProcName)
        {
            StoredProcName = storedProcName;
        }
    }

    public class Repository<TSummary, TFull> : IListRepository<TSummary>, ISingleItemRepository<TFull>
        where TSummary : IPersistent, new()
        where TFull : IPersistent, new()
    {
        public IEnumerable<TSummary> GetAll(IFilter filter = null)
        {
            var attr = (GetAllAttribute)Attribute.GetCustomAttribute(GetType(), typeof(GetAllAttribute));

            if (filter == null)
                filter = EmptyFilter.Instance;

            if (attr == null)
                throw new InvalidOperationException(string.Format("GetAll attribute must be specified on type {0} to use this function. This attribute defines the name of the stored proc to use when Repository.GetAll() is called.", GetType().Name));

            return DbHelper.GetList<TSummary>(attr.StoredProcName, filter.GetParameters().ToArray());
        }

        public TFull GetSingle(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var attr = (GetSingleAttribute)Attribute.GetCustomAttribute(GetType(), typeof(GetSingleAttribute));

            if (attr == null)
                throw new InvalidOperationException(string.Format("GetSingle attribute must be specified on type {0} to use this function. This attribute defines the name of the stored proc to use when Repository.GetSingle() is called.", GetType().Name));

            return DbHelper.GetItem<TFull>(attr.StoredProcName, new DbParameter("id", id));
        }

        public void DeleteSingle(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var attr = (DeleteSingleAttribute)Attribute.GetCustomAttribute(GetType(), typeof(DeleteSingleAttribute));

            if (attr == null)
                throw new InvalidOperationException(string.Format("DeleteSingle attribute must be specified on type {0} to use this function. This attribute defines the name of the stored proc to use when Repository.DeleteSingle() is called.", GetType().Name));

            DbHelper.ExecuteNonQuery(attr.StoredProcName, new DbParameter("id", id));
        }
    }
}