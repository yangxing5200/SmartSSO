﻿using System;
using System.Collections.Generic;
using ServiceStack.Redis;
using SSO.Util;
using IRedisClient = ServiceStack.Redis.IRedisClient;

namespace SSO.Cache
{
    internal class CacheHelper
    {
        private static string RedisPath = System.Configuration.ConfigurationSettings.AppSettings["RedisPath"];

        #region -- 连接信息 --
        public static PooledRedisClientManager prcm
        {
            get
            {
                try
                {
                    if (_prcm == null)
                    {
                        _prcm = CreateManager(new string[] { RedisPath }, new string[] { RedisPath });
                    }
                    return _prcm;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }


        public static PooledRedisClientManager _prcm = null; //CreateManager(new string[] { RedisPath }, new string[] { RedisPath });
        private static PooledRedisClientManager CreateManager(string[] readWriteHosts, string[] readOnlyHosts)
        {
            // 支持读写分离，均衡负载 
            return new PooledRedisClientManager(readWriteHosts, readOnlyHosts, new RedisClientManagerConfig
            {
                MaxWritePoolSize = 5, // “写”链接池链接数 
                MaxReadPoolSize = 5, // “读”链接池链接数 
                AutoStart = true,
            });
        }
        #endregion

        #region 获取键值
        //public static string GetCacheItemKey<M>(string key,bool iscollection) where M:class
        //{
        //    return typeof(M).Name + ":" + key + (iscollection ? "list" : "");
        //}
        #endregion

        #region -- Item --
        /// <summary>
        /// 设置单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool Item_Set<T>(string key, T t)
        {
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    return redis.Set<T>(key, t, new TimeSpan(1, 0, 0));
                }
            }
            catch (Exception ex)
            {
                // LogInfo
            }
            return false;
        }

        /// <summary>
        /// 获取单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Item_Get<T>(string key) where T : class
        {
            if (prcm == null) return null;
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Get<T>(key);
            }
        }

        /// <summary>
        /// 移除单体
        /// </summary>
        /// <param name="key"></param>
        public static bool Item_Remove(string key)
        {
            if (prcm == null) return false;
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Remove(key);
            }
        }

        #endregion

        #region -- List --

        //public static void List_Add<T>(string key, T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        redisTypedClient.AddItemToList(redisTypedClient.Lists[key], t);
        //    }
        //}



        //public static bool List_Remove<T>(string key, T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        return redisTypedClient.RemoveItemFromList(redisTypedClient.Lists[key], t) > 0;
        //    }
        //}
        //public static void List_RemoveAll<T>(string key)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        redisTypedClient.Lists[key].RemoveAll();
        //    }
        //}

        //public static int List_Count(string key)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        return redis.GetListCount(key);
        //    }
        //}

        //public static List<T> List_GetRange<T>(string key, int start, int count)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var c = redis.GetTypedClient<T>();
        //        return c.Lists[key].GetRange(start, start + count - 1);
        //    }
        //}


        //public static List<T> List_GetList<T>(string key)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var c = redis.GetTypedClient<T>();
        //        return c.Lists[key].GetRange(0, c.Lists[key].Count);
        //    }
        //}

        //public static List<T> List_GetList<T>(string key, int pageIndex, int pageSize)
        //{
        //    int start = pageSize * (pageIndex - 1);
        //    return List_GetRange<T>(key, start, pageSize);
        //}

        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        //public static void List_SetExpire(string key, DateTime datetime)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        redis.ExpireEntryAt(key, datetime);
        //    }
        //}
        #endregion

        #region -- Set --
        //public static void Set_Add<T>(string key, T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        redisTypedClient.Sets[key].Add(t);
        //    }
        //}
        //public static bool Set_Contains<T>(string key, T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        return redisTypedClient.Sets[key].Contains(t);
        //    }
        //}
        //public static bool Set_Remove<T>(string key, T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var redisTypedClient = redis.GetTypedClient<T>();
        //        return redisTypedClient.Sets[key].Remove(t);
        //    }
        //}
        #endregion


        #region -- Hash --
        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static bool Hash_Exist<T>(string key, string dataKey)
        {
            if (prcm == null) return false;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    return redis.HashContainsEntry(key, dataKey);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static bool Hash_Set<T>(string key, string dataKey, T t)
        {
            if (prcm == null) return false;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    string value = ServiceStack.Text.JsonSerializer.SerializeToString<T>(t);
                    return redis.SetEntryInHash(key, dataKey, value);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static bool Hash_SetEx<T>(string key, string dataKey, T t)
        {
            if (prcm == null) return false;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    string value = Tools.JsonSerializer(t);
                    //Newtonsoft.Json.JavaScriptConvert.System.Web.Script.Serialization.JavaScriptSerializer.ServiceStack.Text.JsonSerializer.SerializeToString<T>(t);
                    return redis.SetEntryInHash(key, dataKey, value);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static T Hash_GetEx<T>(string key, string dataKey) where T : class
        {
            if (prcm == null) return null;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    string value = redis.GetValueFromHash(key, dataKey);
                    //return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(value);
                    return Tools.JsonDeserialize<T>(value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static bool Hash_Remove(string key, string dataKey)
        {
            if (prcm == null) return false;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    return redis.RemoveEntryFromHash(key, dataKey);
                }

            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 移除整个hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static bool Hash_Remove(string key)
        {
            if (prcm == null) return false;

            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    return redis.Remove(key);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public static T Hash_Get<T>(string key, string dataKey) where T : class
        {
            if (prcm == null) return null;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    string value = redis.GetValueFromHash(key, dataKey);
                    return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取整个hash的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<T> Hash_GetAll<T>(string key) where T : class
        {
            if (prcm == null) return null;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    var list = redis.GetHashValues(key);
                    if (list != null && list.Count > 0)
                    {
                        List<T> result = new List<T>();
                        foreach (var item in list)
                        {
                            var value = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                            result.Add(value);
                        }
                        return result;
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        public static void Hash_SetExpire(string key, DateTime datetime)
        {
            if (prcm == null) return;
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    redis.ExpireEntryAt(key, datetime);
                }

            }
            catch (Exception)
            {

            }
        }
        #endregion



        #region -- SortedSet --
        /// <summary>
        ///  添加数据到 SortedSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="score"></param>
        public static bool SortedSet_Add<T>(string key, T t, double score)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                string value = ServiceStack.Text.JsonSerializer.SerializeToString<T>(t);
                return redis.AddItemToSortedSet(key, value, score);
            }
        }
        /// <summary>
        /// 移除数据从SortedSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool SortedSet_Remove<T>(string key, T t)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                string value = ServiceStack.Text.JsonSerializer.SerializeToString<T>(t);
                return redis.RemoveItemFromSortedSet(key, value);
            }
        }
        /// <summary>
        /// 修剪SortedSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="size">保留的条数</param>
        /// <returns></returns>
        public static long SortedSet_Trim(string key, int size)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.RemoveRangeFromSortedSet(key, size, 9999999);
            }
        }
        /// <summary>
        /// 获取SortedSet的长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long SortedSet_Count(string key)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.GetSortedSetCount(key);
            }
        }

        /// <summary>
        /// 获取SortedSet的分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static List<T> SortedSet_GetList<T>(string key, int pageIndex, int pageSize)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                var list = redis.GetRangeFromSortedSet(key, (pageIndex - 1) * pageSize, pageIndex * pageSize - 1);
                if (list != null && list.Count > 0)
                {
                    List<T> result = new List<T>();
                    foreach (var item in list)
                    {
                        var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                        result.Add(data);
                    }
                    return result;
                }
            }
            return null;
        }


        /// <summary>
        /// 获取SortedSet的全部数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static List<T> SortedSet_GetListALL<T>(string key)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                var list = redis.GetRangeFromSortedSet(key, 0, 9999999);
                if (list != null && list.Count > 0)
                {
                    List<T> result = new List<T>();
                    foreach (var item in list)
                    {
                        var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                        result.Add(data);
                    }
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        public static void SortedSet_SetExpire(string key, DateTime datetime)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                redis.ExpireEntryAt(key, datetime);
            }
        }

        //public static double SortedSet_GetItemScore<T>(string key,T t)
        //{
        //    using (IRedisClient redis = prcm.GetClient())
        //    {
        //        var data = ServiceStack.Text.JsonSerializer.SerializeToString<T>(t);
        //        return redis.GetItemScoreInSortedSet(key, data);
        //    }
        //    return 0;
        //}

        #endregion
    }
}
