//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of data library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Nistec.Collections;
using Nistec.Data;
using System.Threading;
using Nistec.Threading;


namespace Nistec.Data.Entities
{


    /// <summary>
    /// Sync time struct
    /// </summary>
    [Serializable]
    public class ActiveScheduler : EntityView, IScheduler
    {
 
        #region Dispose

        protected override void DisposeInner(bool disposing)
        {

            if ((_aTimer != null) && (Interlocked.Exchange(ref this._aTimer, null) != null))
            {
                _aTimer.Dispose();
                _aTimer = null;
            }

            base.DisposeInner(disposing);
        }

        #endregion



        public const int DefaultInterval = 60000;
        public const string DefaultMappingName = "Schedule";
        private uint _SyncInterval = 30;
        private uint _currentSync = 0;
        private Dictionary<string, DateTime> _actionList;

        public event SchedulerEventHandler ScheduleElapsed;

        public uint SyncInterval
        {
            get { return _SyncInterval; }
            set
            {
                if (value > 5)
                {
                    _SyncInterval = value;
                }
            }
        }
 
        /// <summary>
        /// ActiveScheduler ctor
        /// </summary>
        /// <param name="dalBase"></param>
        public ActiveScheduler(IAutoBase dalBase)
            : this(dalBase.Connection)
        {
        }

        /// <summary>
        /// ActiveScheduler ctor
        /// </summary>
        /// <param name="cnn"></param>
        public ActiveScheduler(IDbConnection cnn)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn");
            }

            //Connection = cnn;
            base.InitEntity(cnn.Database, DefaultMappingName, DefaultMappingName, EntitySourceType.Table, /*cnn.ConnectionString, DBFactory.GetProvider(cnn),*/  EntityKeys.Get("ScheduleId"));
            InitScheduler();
        }
        /// <summary>
        /// ActiveScheduler ctor
        /// </summary>
        /// <param name="db"></param>
        public ActiveScheduler(IDbContext db /*string connecionName, string connecionString, DBProvider provider*/)
        {
            base.EntityDb =new EntityDbContext(db,DefaultMappingName, DefaultMappingName,EntitySourceType.Table, EntityKeys.Get( "ScheduleId" ));
            InitScheduler();
        }

        private void InitScheduler()
        {
            _actionList = new Dictionary<string, DateTime>();
            _aTimer = new System.Timers.Timer();
            _aTimer.AutoReset = true;
            _aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            _aTimer.Interval = (double)DefaultInterval;
            Start();
        }

        #region active view
        [EntityProperty(EntityPropertyType.Identity)]
        public int ScheduleId
        {
            get { return base.GetValue<int>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public string ScheduleName
        {
            get { return base.GetValue<string>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public int Mode//ScheduleMode
        {
            get { return base.GetValue<int>(); }
        }

        [EntityProperty(EntityPropertyType.Default)]
        public int Time
        {
            get { return base.GetValue<int>(); }
        }

        [EntityProperty(EntityPropertyType.Default)]
        public int WeekDay//DayOfWeek
        {
            get { return base.GetValue<int>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public DateTime SpecialDate
        {
            get { return base.GetValue<DateTime>(); }
        }

        [EntityProperty(EntityPropertyType.Default)]
        public DateTime LastTime
        {
            get { return base.GetValue<DateTime>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public DateTime NextTime
        {
            get { return base.GetValue<DateTime>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public DateTime ExpirationTime
        {
            get { return base.GetValue<DateTime>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public bool Enabled
        {
            get { return base.GetValue<bool>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public bool AllowExpired
        {
            get { return base.GetValue<bool>(); }
        }
        [EntityProperty(EntityPropertyType.Default)]
        public int CallCount
        {
            get { return base.GetValue<int>("Count"); }
        }


        #endregion

        #region auto sync

        System.Timers.Timer _aTimer;

        /// <summary>
        /// Get indicator if sync are enabled 
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool TimerEnabled
        {
            get
            {
                if (_aTimer == null)
                    return false;
                return _aTimer.Enabled;
            }
        }

        /// <summary>
        /// Start Async config Background multi thread Listner 
        /// </summary>
        protected virtual void Start()
        {
            if (TimerEnabled)
                return;

            try
            {

                _aTimer.Enabled = true;
                //initilized = true;
                // Keep the timer alive until the end of Main.
                GC.KeepAlive(_aTimer);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }


        /// <summary>
        /// Stop AsyncQueue Background multi thread Listner 
        /// </summary>
        protected virtual void Stop()
        {
            Console.WriteLine("Stop Scheduler ");
            if (_aTimer != null)
            {
                _aTimer.Stop();
            }
        }


        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Start Scheduler ...");
            if (!IsEmpty && Count > 0)
            {
                _currentSync++;
                OnSchedule();
                if (_currentSync > SyncInterval)
                {
                    base.EntityAsyncCmd.AsyncExecute();
                    _currentSync = 0;
                }
            }
        }

        protected override void OnAsyncCompleted(Nistec.Threading.AsyncDataResultEventArgs e)
        {
            base.OnAsyncCompleted(e);

        }


        private void OnSchedule()
        {

            DateTime time = DateTime.Now;
            if (base.IsEmpty)
                return;
            int count = 0;
            for (int i = 0; i < base.Count; i++)
            {
                base.Position = i;
                if (IsValid() && time >= this.NextTime)
                {
                    //curSchedule = item;
                    CalcNextTime();

                    if (!_actionList.ContainsKey(ScheduleName))
                    {
                        _actionList[ScheduleName] = DateTime.Now;
                        OnScheduleElapsed(new SchedulerEventArgs(this,ScheduleName));
                    }
                    count++;
                }
            }
            if (count > 0)
            {
                base.SaveChanges();// Changes();//Connection);
            }
        }

        public void Commit(string scheduleName)
        {
            _actionList.Remove(scheduleName);
        }

        private bool IsValid()
        {
            if (!Enabled)
                return false;
            if (Time <= 0)
                return false;
            if(Mode<0 || Mode> 4)
                return false;

            return true;
        }

        private void CalcNextTime()
        {
            DateTime curTime = NextTime;
            DateTime calcNext=DateTime.Now.AddMinutes(1);
            TimeSpan time = TimeSpan.FromMinutes((double)Time);
            switch ((ScheduleMode)Mode)
            {
                case ScheduleMode.Interval:
                    calcNext = LastTime.AddDays(time.Days).AddHours(time.Hours).AddMinutes(time.Minutes);
                    break;
                case ScheduleMode.Daily:
                    calcNext = LastTime.AddDays(1);
                    break;
                case ScheduleMode.Weekly:
                    calcNext = LastTime.AddDays(7);
                    break;
                case ScheduleMode.Monthly:
                    calcNext = LastTime.AddMonths(1);
                    break;
                case ScheduleMode.Once:
                    base.SetValue("Enabled", 0);
                    break;
                default:
                    return;
            }
            base.SetValue("LastTime",curTime);
            base.SetValue("NextTime", calcNext);
            base.SetValue("Count", CallCount+1);

        }

        /// <summary>
        /// OnTimeElapsed
        /// </summary>
        /// <param name="e"></param>
        private void OnScheduleElapsed(SchedulerEventArgs e)
        {
            if (ScheduleElapsed != null)
                ScheduleElapsed(this, e);
        }


        #endregion

    }
}
