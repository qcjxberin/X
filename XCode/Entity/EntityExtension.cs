﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NewLife;
using NewLife.Collections;
using NewLife.Model;
using NewLife.Reflection;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace XCode
{
    /// <summary>实体扩展方法</summary>
    public static class EntityExtension
    {
        #region 泛型实例列表扩展
        /// <summary>根据指定项查找</summary>
        /// <param name="list">实体列表</param>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        [Obsolete("将来不再支持实体列表，请改用list.FirstOrDefault()")]
        public static T Find<T>(this IList<T> list, String name, Object value) where T : IEntity
        {
            return list.FirstOrDefault(e => e[name] == value);
        }

        /// <summary>根据指定项查找</summary>
        /// <param name="list">实体列表</param>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        [Obsolete("将来不再支持实体列表，请改用list.FirstOrDefault()")]
        public static T FindIgnoreCase<T>(this IList<T> list, String name, String value) where T : IEntity
        {
            return list.FirstOrDefault(e => (e[name] + "").EqualIgnoreCase(value));
        }

        ///// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
        ///// <param name="list">实体列表</param>
        ///// <param name="match">条件</param>
        ///// <returns></returns>
        //[Obsolete("将来不再支持实体列表，请改用list.FirstOrDefault()")]
        //public static T Find<T>(this IList<T> list, Predicate<T> match) where T : IEntity
        //{
        //    return list.FirstOrDefault(e => match(e));
        //}

        /// <summary>根据指定项查找</summary>
        /// <param name="list">实体列表</param>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        [Obsolete("将来不再支持实体列表，请改用list.Where()")]
        public static IList<T> FindAll<T>(this IList<T> list, String name, Object value) where T : IEntity
        {
            return list.Where(e => e[name] == value).ToList();
        }

        /// <summary>根据指定项查找</summary>
        /// <param name="list">实体列表</param>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        [Obsolete("将来不再支持实体列表，请改用list.Where()")]
        public static IList<T> FindAllIgnoreCase<T>(this IList<T> list, String name, String value) where T : IEntity
        {
            return list.Where(e => (e[name] + "").EqualIgnoreCase(value)).ToList();
        }

        ///// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
        ///// <param name="list">实体列表</param>
        ///// <param name="match">条件</param>
        ///// <returns></returns>
        //[Obsolete("将来不再支持实体列表，请改用list.Where()")]
        //public static IList<T> FindAll<T>(this IList<T> list, Predicate<T> match) where T : IEntity
        //{
        //    return list.Where(e => match(e)).ToList();
        //}

        /// <summary>集合是否包含指定项</summary>
        /// <param name="list">实体列表</param>
        /// <param name="name">名称</param>
        /// <param name="value">数值</param>
        /// <returns></returns>
        [Obsolete("将来不再支持实体列表，请改用list.Any()")]
        public static Boolean Exists<T>(this IList<T> list, String name, Object value) where T : IEntity
        {
            return list.Any(e => e[name] == value);

        }

        /// <summary>实体列表转为字典。主键为Key</summary>
        /// <param name="list">实体列表</param>
        /// <param name="valueField">作为Value部分的字段，默认为空表示整个实体对象为值</param>
        /// <returns></returns>
        //[Obsolete("将来不再支持实体列表，请改用Linq")]
        public static IDictionary ToDictionary<T>(this IEnumerable<T> list, String valueField = null) where T : IEntity
        {
            if (list == null || !list.Any()) return new Dictionary<String, String>();

            var type = list.First().GetType();
            var fact = EntityFactory.CreateOperate(type);

            // 构造主键类型和值类型
            var key = fact.Unique;
            var ktype = key.Type;

            if (!valueField.IsNullOrEmpty())
            {
                var fi = fact.Table.FindByName(valueField) as FieldItem;
                if (fi == null) throw new XException("无法找到名为{0}的字段", valueField);

                type = fi.Type;
            }

            // 创建字典
            var dic = typeof(Dictionary<,>).MakeGenericType(ktype, type).CreateInstance() as IDictionary;
            foreach (var item in list)
            {
                var k = item[key.Name];
                if (!dic.Contains(k))
                {
                    if (!valueField.IsNullOrEmpty())
                        dic.Add(k, item[valueField]);
                    else
                        dic.Add(k, item);
                }
            }

            return dic;
        }

        /// <summary>从实体对象创建参数</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public static IDataParameter[] CreateParameter<T>(this T entity) where T : IEntity
        {
            var dps = new List<IDataParameter>();
            if (entity == null) return dps.ToArray();

            var type = entity.GetType();
            var fact = EntityFactory.CreateOperate(type);
            var db = fact.Session.Dal.Db;

            foreach (var item in fact.Fields)
            {
                dps.Add(db.CreateParameter(item.ColumnName ?? item.Name, entity[item.Name], item.Field));
            }

            return dps.ToArray();
        }

        /// <summary>从实体列表创建参数</summary>
        /// <param name="list">实体列表</param>
        /// <returns></returns>
        public static IDataParameter[] CreateParameters<T>(this IEnumerable<T> list) where T : IEntity
        {
            var dps = new List<IDataParameter>();
            if (list == null || !list.Any()) return dps.ToArray();

            var type = list.First().GetType();
            var fact = EntityFactory.CreateOperate(type);
            var db = fact.Session.Dal.Db;

            foreach (var item in fact.Fields)
            {
                var vs = list.Select(e => e[item.Name]).ToArray();
                dps.Add(db.CreateParameter(item.ColumnName ?? item.Name, vs, item.Field));
            }

            return dps.ToArray();
        }
        #endregion

        #region 对象操作
        /// <summary>把整个集合插入到数据库</summary>
        /// <param name="list">实体列表</param>
        /// <param name="useTransition">是否使用事务保护</param>
        /// <returns></returns>
        public static Int32 Insert<T>(this IEnumerable<T> list, Boolean? useTransition = null) where T : IEntity
        {
            // 避免列表内实体对象为空
            var entity = list.FirstOrDefault(e => e != null);
            if (entity == null) return 0;

            if (list.Count() > 1)
            {
                var fact = entity.GetType().AsFactory();
                var db = fact.Session.Dal.Db;

                // Oracle/MySql批量插入
                if (db.Type == DatabaseType.MySql || db.Type == DatabaseType.Oracle) return BatchInsert(list);
            }

            return DoAction(list, useTransition, e => e.Insert());
        }

        /// <summary>把整个集合更新到数据库</summary>
        /// <param name="list">实体列表</param>
        /// <param name="useTransition">是否使用事务保护</param>
        /// <returns></returns>
        public static Int32 Update<T>(this IEnumerable<T> list, Boolean? useTransition = null) where T : IEntity
        {
            return DoAction(list, useTransition, e => e.Update());
        }

        /// <summary>把整个保存更新到数据库</summary>
        /// <param name="list">实体列表</param>
        /// <param name="useTransition">是否使用事务保护</param>
        /// <returns></returns>
        public static Int32 Save<T>(this IEnumerable<T> list, Boolean? useTransition = null) where T : IEntity
        {
            /*
           * Save的几个场景：
           * 1，Find, Update()
           * 2，new, Insert()
           * 3，new, InsertOrUpdate
           */

            // 避免列表内实体对象为空
            var entity = list.FirstOrDefault(e => e != null);
            if (entity == null) return 0;

            var rs = 0;
            if (list.Count() > 1)
            {
                var fact = entity.GetType().AsFactory();
                var db = fact.Session.Dal.Db;

                // Oracle/MySql批量插入
                if (db.Type == DatabaseType.MySql || db.Type == DatabaseType.Oracle)
                {
                    // 根据是否来自数据库，拆分为两组
                    var ts = Split(list);
                    list = ts.Item1;
                    var es = ts.Item2;
                    foreach (IEntity item in es)
                    {
                        if (item is EntityBase entity2) entity2.Valid(item.IsNullKey);
                    }
                    rs += BatchSave(fact, es);
                }
            }

            return rs + DoAction(list, useTransition, e => e.Save());
        }

        /// <summary>把整个保存更新到数据库，保存时不需要验证</summary>
        /// <param name="list">实体列表</param>
        /// <param name="useTransition">是否使用事务保护</param>
        /// <returns></returns>
        public static Int32 SaveWithoutValid<T>(this IEnumerable<T> list, Boolean? useTransition = null) where T : IEntity
        {
            // 避免列表内实体对象为空
            var entity = list.FirstOrDefault(e => e != null);
            if (entity == null) return 0;

            var rs = 0;
            if (list.Count() > 1)
            {
                var fact = entity.GetType().AsFactory();
                var db = fact.Session.Dal.Db;

                // Oracle/MySql批量插入
                if (db.Type == DatabaseType.MySql || db.Type == DatabaseType.Oracle)
                {
                    // 根据是否来自数据库，拆分为两组
                    var ts = Split(list);
                    list = ts.Item1;
                    rs += BatchSave(fact, ts.Item2);
                }
            }

            return rs + DoAction(list, useTransition, e => e.SaveWithoutValid());
        }

        private static Tuple<IList<T>, IList<T>> Split<T>(IEnumerable<T> list) where T : IEntity
        {
            var updates = new List<T>();
            var others = new List<T>();
            foreach (var item in list)
            {
                if (item.IsFromDatabase)
                    updates.Add(item);
                else
                    others.Add(item);
            }

            return new Tuple<IList<T>, IList<T>>(updates, others);
        }

        private static Int32 BatchSave<T>(IEntityOperate fact, IEnumerable<T> list) where T : IEntity
        {
            // 没有其它唯一索引，且主键为空时，走批量插入
            var rs = 0;
            if (!fact.Table.DataTable.Indexes.Any(di => di.Unique))
            {
                var adds = new List<T>();
                var others = new List<T>();
                foreach (var item in list)
                {
                    if (item.IsNullKey)
                        adds.Add(item);
                    else
                        others.Add(item);
                }
                list = others;

                if (adds.Count > 0) rs += BatchInsert(adds);
            }

            if (list.Any()) rs += InsertOrUpdate(list);

            return rs;
        }

        /// <summary>把整个集合从数据库中删除</summary>
        /// <param name="list">实体列表</param>
        /// <param name="useTransition">是否使用事务保护</param>
        /// <returns></returns>
        public static Int32 Delete<T>(this IEnumerable<T> list, Boolean? useTransition = null) where T : IEntity
        {
            return DoAction(list, useTransition, e => e.Delete());
        }

        private static Int32 DoAction<T>(this IEnumerable<T> list, Boolean? useTransition, Func<T, Int32> func) where T : IEntity
        {
            if (!list.Any()) return 0;

            // 避免列表内实体对象为空
            var entity = list.First(e => e != null);
            if (entity == null) return 0;

            var fact = EntityFactory.CreateOperate(entity.GetType());

            // SQLite 批操作默认使用事务，其它数据库默认不使用事务
            if (useTransition == null) useTransition = fact.Session.Dal.DbType == DatabaseType.SQLite;

            var count = 0;
            if (useTransition != null && useTransition.Value)
            {
                using (var trans = fact.CreateTrans())
                {
                    count = DoAction(list, func, count);

                    trans.Commit();
                }
            }
            else
            {
                count = DoAction(list, func, count);
            }

            return count;
        }

        private static Int32 DoAction<T>(this IEnumerable<T> list, Func<T, Int32> func, Int32 count) where T : IEntity
        {
            // 加锁拷贝，避免遍历时出现多线程冲突
            var arr = list is ICollection<T> cs ? cs.ToArray() : list.ToArray();
            foreach (var item in arr)
            {
                if (item != null) count += func(item);
            }
            return count;
        }
        #endregion

        #region 批量更新
        /// <summary>批量插入</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="columns">要插入的字段，默认所有字段</param>
        /// <param name="list">实体列表</param>
        /// <returns></returns>
        public static Int32 BatchInsert<T>(this IEnumerable<T> list, IDataColumn[] columns = null) where T : IEntity
        {
            var entity = list.First();
            var fact = entity.GetType().AsFactory();
            if (columns == null) columns = fact.Fields.Select(e => e.Field).ToArray();

            return fact.Session.Dal.Session.Insert(columns, list.Cast<IIndexAccessor>());
        }

        /// <summary>批量插入或更新</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="list">实体列表</param>
        /// <param name="columns">要插入的字段，默认所有字段</param>
        /// <param name="updateColumns">主键已存在时，要更新的字段</param>
        /// <param name="addColumns">主键已存在时，要累加更新的字段</param>
        /// <returns></returns>
        public static Int32 InsertOrUpdate<T>(this IEnumerable<T> list, IDataColumn[] columns = null, ICollection<String> updateColumns = null, ICollection<String> addColumns = null) where T : IEntity
        {
            var entity = list.First();
            var fact = entity.GetType().AsFactory();
            if (columns == null) columns = fact.Fields.Select(e => e.Field).ToArray();
            //if (updateColumns == null) updateColumns = entity.Dirtys.Keys;
            if (updateColumns == null)
            {
                // 所有实体对象的脏字段作为更新字段
                var hs = new HashSet<String>();
                foreach (var item in list)
                {
                    foreach (var elm in item.Dirtys)
                    {
                        // 创建时间等字段不参与Update
                        if (elm.Key.StartsWithIgnoreCase("Create")) continue;

                        if (!hs.Contains(elm.Key)) hs.Add(elm.Key);
                    }
                }
                updateColumns = hs;
            }
            if (addColumns == null) addColumns = fact.AdditionalFields;

            return fact.Session.Dal.Session.InsertOrUpdate(columns, updateColumns, addColumns, list.Cast<IIndexAccessor>());
        }

        /// <summary>批量插入或更新</summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="columns">要插入的字段，默认所有字段</param>
        /// <param name="updateColumns">主键已存在时，要更新的字段</param>
        /// <param name="addColumns">主键已存在时，要累加更新的字段</param>
        /// <returns></returns>
        public static Int32 InsertOrUpdate(this IEntity entity, IDataColumn[] columns = null, ICollection<String> updateColumns = null, ICollection<String> addColumns = null)
        {
            var fact = entity.GetType().AsFactory();
            if (columns == null) columns = fact.Fields.Select(e => e.Field).ToArray();
            if (updateColumns == null) updateColumns = entity.Dirtys.Keys;
            if (addColumns == null) addColumns = fact.AdditionalFields;

            return fact.Session.Dal.Session.InsertOrUpdate(columns, updateColumns, addColumns, new[] { entity as IIndexAccessor });
        }
        #endregion
    }
}