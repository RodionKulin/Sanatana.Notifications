using SignaloBot.DAL.Context;
using Common.EntityFramework.ChangeNotifier;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SignaloBot.Sender.Queue.InsertNotifier
{
    public class SqlInsertNotifier<TEntity, TContext> : IStorageInsertNotifier
        where TEntity : class
        where TContext : DbContext
    {
        //поля
        EntityChangeNotifier<TEntity, TContext> _sqlNotifier;
        bool _isStarted;
        

        //свойства
        public ICommonLogger Logger { get; set; }
        public bool HasUpdates { get; set; }



        //инициализация
        public SqlInsertNotifier(ICommonLogger logger = null, params object[] dbContextParams)
        {
            Logger = logger;

            /*  Условий нет, мониторим сразу все сущности в таблице.
              
                Или другой вариант, который можно реализовать:
                Сравнивать с GetUtcDate из колонки таблицы для сообщений.
                Но в процедуру нельзя недетерменированную функцию ставить.
            */

            _sqlNotifier = new EntityChangeNotifier<TEntity, TContext>(
                p => true, dbContextParams);

            _sqlNotifier.Changed += OnSqlNotifierChanged;
            _sqlNotifier.Error += OnSqlNotifierError;
        }


        //методы
        private void OnSqlNotifierChanged(object sender, EntityChangeEventArgs<TEntity> e)
        {
            if (e.Info == SqlNotificationInfo.Insert
                || e.Info == SqlNotificationInfo.Update
                || e.Info == SqlNotificationInfo.Merge)
            {
                HasUpdates = true;
                e.ContinueListening = false;
                _isStarted = false;
            }
        }

        private void OnSqlNotifierError(object sender, NotifierErrorEventArgs e)
        {
            if (Logger != null)
            {
                Logger.Error("Ошибка в {0} по причине {1}. Sql: {2}", GetType().Name, e.Reason, e.Sql);
            }

            _isStarted = false;
        }

        public void StartMonitor()
        {
            if (!_isStarted)
            {
                try
                {
                    _sqlNotifier.RegisterNotification();
                    _isStarted = true;
                }
                catch (Exception exception)
                {
                    if (Logger != null)
                        Logger.Exception(exception);
                }
            }
        }

        public void StopMonitor()
        {
        }


        //IDisposable
        public void Dispose()
        {
            _sqlNotifier.Changed -= OnSqlNotifierChanged;
            _sqlNotifier.Error -= OnSqlNotifierError;

            _sqlNotifier.Dispose();
        }
    }
}
