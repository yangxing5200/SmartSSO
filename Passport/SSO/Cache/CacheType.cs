namespace SSO.Cache
{
    /// <summary>
    /// 缓存对象类型 
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 用户相关类型，每个用户都有一个缓存值，缓存主键是 “关键字 和 用户ID”组合方式
        /// </summary>
        UserCacheType,

        /// <summary>
        /// 实体相关类型，每个实体一个缓存值，缓存主键是 “关键字 和 实体ID”组合方式
        /// </summary>
        EntityCacheType
    }
}
