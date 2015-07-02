namespace SSO.Cache
{
    public class CacheFactory
    {

        public static void AddCache<T>(string cacheKey, T t) where T : class
        {
          //  CacheHelper.
        }

        /// <summary>
        /// 获取实体缓存对象
        /// </summary>
        /// <typeparam name="T">缓存对象类型</typeparam>
        /// <param name="cachekey">主键关键字</param>
        /// <param name="statusCode">状态编码</param>
        /// <returns></returns>
        public static T GetObjListCache<T>(string cachekey) where T : class
        {
            return CacheHelper.Hash_Get<T>(cachekey, cachekey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cachekey">主键关键字</param>
        /// <param name="etyId">实体id</param>
        /// <returns></returns>
        public static T GetObjListCache<T>(string cachekey, string etyId) where T : class
        {
            return CacheHelper.Hash_Get<T>(cachekey, etyId);
        }


    }
}
