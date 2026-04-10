using {{ prefix }}.Core.Common.Enums;
using {{ prefix }}.Core.Common.Repsitories.Entities;
using {{ prefix }}.Core.Common.Response;
using {{ prefix }}.Core.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace {{ prefix }}.Core.Common.Repsitories.Implementation
{
   public class  EFRepository<T> : IEFRepository<T>
         where T : class
    {
         private DbContext _context;

        private DbSet<T> _entities;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        public EFRepository(DbContext context)
        {
            _context = context;
        }

        public DbContext GetDBContext()
        {
            if (_context != null)
            {
                return _context;
            }
            else
            {
                throw new InvalidOperationException(nameof(GetDBContext));
            }
        }

        public EFRepository()
        {
        }

        public virtual IQueryable<T> Table
        {
            get
            {
                return Entities;
            }
        }

        private DbSet<T> Entities
        {
            get
            {
                _entities = _context.Set<T>();
                return _entities;
            }
        }

        /// <summary>
        /// </summary>
        /// 新增
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> Add(T entity)
        {
            await _context.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> AddAndGetId<T1>(T1 entity) where T1 : ObjectEntity
        {
            await _context.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0 ? entity.Id : 0;
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> AddRange(List<T> entity)
        {
            await _context.AddRangeAsync(entity);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual int DelBatch(List<T> list)
        {
            var entities = _context.Set<T>();
            foreach (T it in list)
            {
                entities.Attach(it);
                entities.Remove(it);
            }
            return _context.SaveChanges();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public async Task<int> Delete(Expression<Func<T, bool>> expression)
        {
            var entitys = _context.Set<T>()
                .Where(expression)
                .ToList();
            _context.RemoveRange(entitys);
            return await _context.SaveChangesAsync();
        }

        public int Delete(T entity)
        { 
                if (entity == null)
                    throw new ArgumentNullException("entity");

                var entities = _context.Set<T>();
                entities.Attach(entity);
                entities.Remove(entity);
                return _context.SaveChanges();
           
        }

        /// <summary>
        /// 7.0 执行sql语句 +int ExcuteSql(string strSql, params object[] paras)
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public virtual int ExcuteSql(string strSql, params object[] paras)
        { 
                return _context.Database.ExecuteSqlRaw(strSql, paras);
         
        }

        public virtual T? FirstOrDefault(Expression<Func<T, bool>> whereLambda)
        {
            return this.Entities.FirstOrDefault(whereLambda);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity?> Get<TEntity>(int? id)
            where TEntity : ObjectEntity
        { 
                return await _context.Set<TEntity>().FirstOrDefaultAsync(it => it.Id == id);
            
        }

        /// <summary>
        /// 条件查询单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<T?> Get(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(expression);
        }


        /// <summary>
        /// 条件查询单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<T> Get(Func<IQueryable<T>, IQueryable<T>>? expression = null)
        {
            var result = default(T); 
            var data = _context.Set<T>().AsQueryable();
            if (expression != null)
            {
                data = expression(data);
                result = await data.FirstOrDefaultAsync();
            }
            return result; 
        }

        public T? GetById(object id)
        {
            return this.Entities.Find(id);
        }

        public virtual IQueryable<T> GetDbQuery(Expression<Func<T, bool>>? whereLambda)
        { 
            return _context.Set<T>().WhereIF(whereLambda is not null, whereLambda); 
        }

        /// <summary>
        /// 条件查询所有对象
        /// </summary>
        /// <param name="expression">sh</param>
        /// <returns></returns>
        public async Task<List<T>> GetList(Func<IQueryable<T>, IQueryable<T>>? expression = null)
        {
            IQueryable<T> data;

            data = _context.Set<T>().AsQueryable().AsNoTracking();

            if (expression != null)
            {
                data = expression(data);
            }
            return await data.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// 条件查询所有对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<List<T>> GetList(Expression<Func<T, bool>>? expression)
        {
            if (expression is null)
            {
                return await _context.Set<T>().ToListAsync();
            }
            else
            {
                return await _context.Set<T>().Where(expression).ToListAsync();
            }
        }

        /// <summary>
        /// 5.0 根据条件查询 +List<T> GetListBy(Expression<Func<T,bool>> whereLambda)
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public virtual List<T> GetListBy(Expression<Func<T, bool>> whereLambda)
        {
            return this.Entities.AsNoTracking().Where(whereLambda).ToList();
        }

        /// <summary>
        /// 5.1 根据条件 排序 和查询
        /// </summary>
        /// <typeparam name="TKey">排序字段类型</typeparam>
        /// <param name="whereLambda">查询条件 lambda表达式</param>
        /// <param name="orderLambda">排序条件 lambda表达式</param>
        /// <returns></returns>
        public async Task<List<T>> GetListBy<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderLambda)
        {
            return this.Entities.Where(whereLambda).OrderBy(orderLambda).ToList();
        }

        public async Task<T?> GetByOrder<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T?, TKey>> orderLambda, OrderByTypeEnum orderEnum)
        {
            switch (orderEnum)
            {
                case OrderByTypeEnum.Asc:
                    return await this.Entities.Where(whereLambda).OrderBy(orderLambda).FirstOrDefaultAsync();

                case OrderByTypeEnum.Desc:
                    return await this.Entities.Where(whereLambda).OrderByDescending(orderLambda).FirstOrDefaultAsync();

                default:
                    return null;
            }
        }

        public List<T> GetListWithSql(string sql, params object[] parameters)
        {
            return _context.Set<T>().FromSqlRaw<T>(sql, parameters).ToList();
        }

        public T? GetModel(Expression<Func<T, bool>> whereLambda)
        {
            return this.Entities.AsNoTracking().SingleOrDefault(whereLambda);
        }

        /// <summary>
        /// 8.0 根据条件获取一个 不被ef跟踪的 对象
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public virtual T GetModelWithOutTrace(Expression<Func<T, bool>> whereLambda)
        {
            return _context.Set<T>().AsNoTracking().Single(whereLambda);
        }

        /// <summary>
        ///9.0 EF操作原始sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public T GetModelWithSql(string sql, params object[] parameters)
        {
            return _context.Set<T>().FromSqlRaw<T>(sql, parameters).Single();
        }

        /// <summary>
        /// 6.0 分页查询 + List<T> GetPagedList<TKey>
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="whereLambda">条件 lambda表达式</param>
        /// <param name="orderBy">排序 lambda表达式</param>
        /// <returns></returns>
        public virtual List<T> GetPagedList<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy)
        {
            // 分页 一定注意： Skip 之前一定要 OrderBy
            return this.Entities.Where(whereLambda).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }



        /// <summary>
        /// 6.1分页查询 带输出
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="rowCount"></param>
        /// <param name="whereLambda"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public virtual List<T> GetPagedList<TKey>(int pageIndex, int pageSize, ref int rowCount, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy, bool isAsc = true)
        {
            //查询总行数
            rowCount = this.Entities.Where(whereLambda).Count();
            //查询分页数据
            if (isAsc)
            {
                return this.Entities.OrderBy(orderBy).Where(whereLambda).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                return this.Entities.OrderByDescending(orderBy).Where(whereLambda).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        /// <summary>
        /// 分页查询数据1
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public async Task<Page<T>> GetPageList(int page, int size, Func<IQueryable<T>, IQueryable<T>>? expression, Expression<Func<T, T>> selector = null, Expression<Func<T, object>> funcSort = null, OrderByTypeEnum orderByType = OrderByTypeEnum.Asc)
        {
            var res = new Page<T>();
            var data = _context.Set<T>().AsNoTracking().Where(e => 1 == 1);
            if (expression is not null)
            {
                data = expression(data);
            }
            if (funcSort is not null)
            {
                data = orderByType == OrderByTypeEnum.Asc ? data.OrderBy(funcSort) : data.OrderByDescending(funcSort);
            }
            var count = await data.CountAsync();
            res = new Page<T>()
            {
                Total = count,
                Items = await data.Skip((page - 1) * size).Take(size).ToListAsync(),
                PageCount = count.GetCeiling(size),
                PageIndex = page,
                PageSize = size,
            };
            return res;
        }

        public bool Insert(T entity)
        {
            var entities = _context.Set<T>();
            if (entity == null)
                throw new ArgumentNullException("entity");
            entities.Add(entity);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// 新增 实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int InsertBatch(List<T> list)
        {
            foreach (T t in list)
            {
                this.Entities.Add(t);
            }
            return this._context.SaveChanges();
        }

        public IEFRepository<T> Instance(DbContext context)
        {
            _context = context;
            return this;
        }

        public bool IsUpdate(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            return _context.SaveChanges() > 0;
        }

        public int Modify(T model, params string[] proNames)
        { 
                int botInt;
                try
                {
                    //4.1将 对象 添加到 EF中
                    var entry = _context.Entry<T>(model);
                    //4.2先设置 对象的包装 状态为 Unchanged
                    entry.State = EntityState.Unchanged;
                    //4.3循环 被修改的属性名 数组
                    foreach (string proName in proNames)
                    {
                        //4.4将每个 被修改的属性的状态 设置为已修改状态;后面生成update语句时，就只为已修改的属性 更新
                        entry.Property(proName).IsModified = true;
                        //entry.Properties(proName).
                    }
                    botInt = _context.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }
                //4.4一次性 生成sql语句到数据库执行
                return botInt; 
        }

        public int Modifys(List<T> models, params string[] proNames)
        {
            int botInt;
            try
            {
                foreach (var item in models)
                {
                    //4.1将 对象 添加到 EF中
                    var entry = _context.Entry<T>(item);
                    //4.2先设置 对象的包装 状态为 Unchanged
                    entry.State = EntityState.Unchanged;
                    //4.3循环 被修改的属性名 数组
                    foreach (string proName in proNames)
                    {
                        //4.4将每个 被修改的属性的状态 设置为已修改状态;后面生成update语句时，就只为已修改的属性 更新
                        entry.Property(proName).IsModified = true;
                        //entry.Properties(proName).
                    }
                }

                botInt = _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            //4.4一次性 生成sql语句到数据库执行
            return botInt;
        }

        /// <summary>
        /// 4.0 批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        public virtual int ModifyBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
        {
            //4.1查询要修改的数据
            List<T> listModifing = this.Entities.Where(whereLambda).ToList();

            //获取 实体类 类型对象
            Type t = typeof(T);
            //获取 实体类 所有的 公有属性
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            //创建 实体属性 字典集合
            Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
            //将 实体属性 中要修改的属性名 添加到 字典集合中 键：属性名  值：属性对象
            proInfos.ForEach(p =>
            {
                if (modifiedProNames.Contains(p.Name))
                {
                    dictPros.Add(p.Name, p);
                }
            });

            //4.3循环 要修改的属性名
            foreach (string proName in modifiedProNames)
            {
                //判断 要修改的属性名是否在 实体类的属性集合中存在
                if (dictPros.ContainsKey(proName))
                {
                    //如果存在，则取出要修改的 属性对象
                    PropertyInfo proInfo = dictPros[proName];
                    //取出 要修改的值
                    object? newValue = proInfo.GetValue(model, null);

                    //4.4批量设置 要修改 对象的 属性
                    foreach (T usrO in listModifing)
                    {
                        //为 要修改的对象 的 要修改的属性 设置新的值
                        proInfo.SetValue(usrO, newValue, null);
                    }
                }
            }
            //4.4一次性 生成sql语句到数据库执行
            return this._context.SaveChanges();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> Update(T entity)
        { 
            _context.Update(entity);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateRange(List<T> entity)
        {
            _context.UpdateRange(entity);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateRange(T entity)
        {
            _context.Update(entity);
            return await _context.SaveChangesAsync();
        }

        public Task<int> DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<Page<TResult>> GetPageList<TResult>(Func<IQueryable<T>, Page<TResult>> func)
        {
            return Task.FromResult(func.Invoke(this.Entities));
        }

    }
}
