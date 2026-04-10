using {{ prefix }}.Core.Common.Enums;
using {{ prefix }}.Core.Common.Repsitories.Entities;
using {{ prefix }}.Core.Common.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace {{ prefix }}.Core.Common.Repsitories
{
     public interface IEFRepository<T>
       where T : class
    {
       
        /// <summary>
        /// 提供对数据类型已知的特定数据源的查询进行计算的功能。
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> Add(T entity);

        public Task<int> AddAndGetId<T1>(T1 entity) where T1 : ObjectEntity;

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> AddRange(List<T> entity);
        public Task<Page<TResult>> GetPageList<TResult>(Func<IQueryable<T>, Page<TResult>> func);

        /// <summary>
        /// 删除集合数据
        /// </summary>
        /// <param name="list">实体模型集合</param>
        /// <returns>返回影响行数</returns>
        int DelBatch(List<T> list);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="entity">实体模型</param>
        /// <returns>返回影响行数</returns>
        int Delete(T entity);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        Task<int> Delete(Expression<Func<T, bool>>? expression);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="entity"></typeparam>
        /// <returns></returns>
        Task<int> DeleteAsync(T entity);

        /// <summary>
        ///执行sql语句
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <param name="paras">参数</param>
        /// <returns>返回影响行数</returns>
        int ExcuteSql(string strSql, params object[] paras);

        /// <summary>
        /// 根据Lambda表达式获取第一条数据
        /// </summary>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <returns>模型实体</returns>
        T? FirstOrDefault(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 条件查询单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<T> Get(Expression<Func<T, bool>> expression);

        /// <summary>
        /// 条件查询单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<T> Get(Func<IQueryable<T>, IQueryable<T>>? expression = null);

        /// 根据编号获取模型实体
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns>返回模型实体</returns>
        T? GetById(object id);

        /// <summary>
        /// 获取DBContext
        /// </summary>
        /// <returns></returns>
        DbContext GetDBContext();

        /// <summary>
        /// 表示针对 DbContext 的 LINQ to Entities 查询。
        /// </summary>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <returns>返回LINQ to Entities 查询</returns>
        IQueryable<T> GetDbQuery(Expression<Func<T, bool>>? whereLambda);

        /// <summary>
        /// 条件查询所有对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Task<List<T>> GetList(Expression<Func<T, bool>>? expression);

        /// <summary>
        /// 条件查询所有对象
        /// </summary>
        /// <param name="expression">sh</param>
        /// <returns></returns>
        public Task<List<T>> GetList(Func<IQueryable<T>, IQueryable<T>>? expression = null);

        /// <summary>
        /// 根据Lambda表达式获取数据
        /// </summary>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <returns>返回模型实体集合</returns>
        List<T> GetListBy(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 根据Lambda表达式获取排序后的数据集合
        /// </summary>
        /// <typeparam name="TKey">排序的类型</typeparam>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <param name="orderLambda">Lambda表达式</param>
        /// <returns>返回实体模型集合</returns>
        Task<List<T>> GetListBy<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderLambda);
        /// <summary>
        /// 根据排序获取第一个或最后一个
        /// </summary>
        /// <typeparam name="TKey">类型</typeparam>
        /// <param name="orderLambda">Lambda排序的字段</param>
        /// <param name="orderEnum">顺序枚举</param>
        /// <returns></returns>
        Task<T?> GetByOrder<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T?, TKey>> orderLambda, OrderByTypeEnum orderEnum);

        List<T> GetListWithSql(string sql, params object[] parameters);

        /// <summary>
        /// 根据Lambda表达式获取模型实体
        /// </summary>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <returns>返回模型实体</returns>
        T? GetModel(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 不采用跟踪获取实体模型
        /// </summary>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <returns>返回实体模型</returns>
        T GetModelWithOutTrace(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 根据sql语句获取模型实体集合
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">sql参数</param>
        /// <returns>返回模型实体集合</returns>
        T GetModelWithSql(string sql, params object[] parameters);

        /// <summary>
        /// 数据分页
        /// </summary>
        /// <typeparam name="TKey">排序类型</typeparam>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">显示条数</param>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <param name="orderBy">Lambda表达式</param>
        /// <returns>返回实体模型集合</returns>
        List<T> GetPagedList<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda,
                                   Expression<Func<T, TKey>> orderBy);

        /// <summary>
        /// 数据分页
        /// </summary>
        /// <typeparam name="TKey">排序类型</typeparam>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">显示条数</param>
        /// <param name="rowCount">总条数</param>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <param name="orderBy">Lambda表达式</param>
        /// <param name="isAsc">是否排序</param>
        /// <returns>返回实体模型集合</returns>
        List<T> GetPagedList<TKey>(int pageIndex, int pageSize, ref int rowCount, Expression<Func<T, bool>> whereLambda,
                                   Expression<Func<T, TKey>> orderBy, bool isAsc = true);

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public Task<Page<T>> GetPageList(int page, int size, Func<IQueryable<T>, IQueryable<T>>? expression, Expression<Func<T, T>> selector = null, Expression<Func<T, object>> funcSort = null, OrderByTypeEnum orderByType = OrderByTypeEnum.Asc);

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="entity">实体模型</param>
        /// <returns>返回是否成功</returns>
        bool Insert(T entity);

        /// <summary>
        /// 添加列表数据
        /// </summary>
        /// <param name="list">实体模型集合</param>
        /// <returns>返回影响行数</returns>
        int InsertBatch(List<T> list);

        /// <summary>
        /// 实例化DbContext
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <returns>返回IEFRepository</returns>
        IEFRepository<T> Instance(DbContext context);

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="entity">实体模型</param>
        /// <returns>返回执行成功</returns>
        bool IsUpdate(T entity);

        /// <summary>
        /// 修改指定属性
        /// </summary>
        /// <param name="model">实体模型</param>
        /// <param name="proNames">属性名称</param>
        /// <returns>返回影响行数</returns>
        int Modify(T model, params string[] proNames);

        /// <summary>
        /// 根据Lambda表达式修改指定属性
        /// </summary>
        /// <param name="model">实体模型</param>
        /// <param name="whereLambda">Lambda表达式</param>
        /// <param name="modifiedProNames">需要修改的属性</param>
        /// <returns>返回影响行数</returns>
        int ModifyBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames);

        int Modifys(List<T> models, params string[] proNames);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> Update(T entity);

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> UpdateRange(List<T> entity);

    }
}
