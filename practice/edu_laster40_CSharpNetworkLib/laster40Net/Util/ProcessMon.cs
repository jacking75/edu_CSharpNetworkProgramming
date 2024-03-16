using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace laster40Net.Util
{
    /// <summary>
    /// 프로세스 정보
    /// </summary>
    public struct ProcessInfo
    {
        public string name;         // 프로세스 이름
        public int pid;             // pid
        public double cpu;          // cpu 점유률
        public double mem;          // 메모리 사용량
        public int threadCount;     // 스레드수
        public string filePath;     // 파일 경로
        public TimeSpan totalProcessorTime; // 전체 프로세서 시간(cpu usage 계산에 사용)
        public DateTime startTime;  //시작 시간
        public int handleCount;     // 핸들 카운터
    }

    public delegate void ProcessMonEvent(ProcessInfo pi);
    public delegate void ProcessMonPostUpdate();
    public delegate void ProcessMonIdleEvent();

    /// <summary>
    /// 프로세스 모니터링
    /// </summary>
    public class ProcessMon
    {
        private DateTime _lastUpdated;
        private ProcessInfo[] _lists = null;
        private int _listIdx = 0;

        public event ProcessMonEvent NewProcessEvent;
        public event ProcessMonEvent UpdateProcessEvent;
        public event ProcessMonEvent CloseProcessEvent;
        public event ProcessMonPostUpdate PostUpdateEvent;
        public event ProcessMonIdleEvent IdleEvent;
        

        private Thread _thread;
        private int _updateIntval;
        private AtomicInt _isStop = new AtomicInt();
        private AtomicInt _isRunning = new AtomicInt();

        private Object _syncSnapShotProcessInfo = new Object();
        private ProcessInfo[] _snapShotProcessInfo = null;

        public ProcessMon(int updateIntval)
        {
            this._updateIntval = updateIntval;
        }

        public bool Start()
        {
            if (!_isRunning.CasOn())
                return false;

            _snapShotProcessInfo = null;

            try
            {
                _thread = new Thread(ThreadEntry);
                _thread.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Stop()
        {
            _isStop.On();
            _thread.Join();
        }

        private void ThreadEntry()
        {
            int tick = Environment.TickCount;
            while (!_isStop.IsOn())
            {
                int cur = Environment.TickCount;
                if (cur - tick > _updateIntval)
                {
                    Update();
                    tick = Environment.TickCount;
                }
                else
                {
                    // 좀 쉬어줘~
                    Thread.Sleep(50);
                    if (IdleEvent != null)
                        IdleEvent();
                }
            }
        }

        private double GetProcessCpuUsage(TimeSpan current, TimeSpan last)
        {
            TimeSpan procTime = last;
            double cpuDiff = current.TotalMilliseconds - procTime.TotalMilliseconds;
            TimeSpan timeDiff = DateTime.Now - _lastUpdated;
            double cpuUsage = cpuDiff / timeDiff.TotalMilliseconds * 100 / Environment.ProcessorCount;
            return cpuUsage;
        }

        public void Update()
        {
            System.Diagnostics.Process[] currentProcesses = System.Diagnostics.Process.GetProcesses();

            UpdateOldProcesses(currentProcesses);
            UpdateNewProcesses(currentProcesses);

            MakeShapShotProcessInfo();

            _lastUpdated = DateTime.Now;

            // 업데이트 이후에 작업처리
            if (PostUpdateEvent!=null)
                PostUpdateEvent();
        }

        public ProcessInfo[] GetShapShotProcessInfo()
        {
            ProcessInfo[] snapShot = null;
            // 스냅샷 만들어 놓기
            lock (_syncSnapShotProcessInfo)
            {
                if (_snapShotProcessInfo == null)
                    return null;
                snapShot = new ProcessInfo[_snapShotProcessInfo.Length];
                Array.Copy(_snapShotProcessInfo, snapShot, _snapShotProcessInfo.Length);
            }
            return snapShot;
        }

        private void MakeShapShotProcessInfo()
        {    
            // 스냅샷 만들어 놓기
            lock (_syncSnapShotProcessInfo)
            {
                if (_lists != null)
                {
                    _snapShotProcessInfo = new ProcessInfo[_lists.Length];
                    Array.Copy(_lists, _snapShotProcessInfo, _lists.Length);
                }
            }
        }

        private System.Diagnostics.Process GetProcessByPid(System.Diagnostics.Process[] processes, int pid)
        {
            System.Diagnostics.Process match = System.Array.Find(processes, (System.Diagnostics.Process prc) => prc.Id == pid);
            return match;
        }

        private void UpdateOldProcesses(System.Diagnostics.Process[] currentProcesses)
        {
            if (_lists == null)
            {
                _lists = new ProcessInfo[currentProcesses.Length];
                return;
            }

            ProcessInfo[] currentProcessInfo = new ProcessInfo[currentProcesses.Length];
            _listIdx = 0;

            foreach (var prc in _lists)
            {
                System.Diagnostics.Process exist = GetProcessByPid(currentProcesses, prc.pid);
                if (exist != null)
                {
                    if (UpdateProcessInfo(ref currentProcessInfo[_listIdx], exist, prc))
                    {
                        if (UpdateProcessEvent != null)
                            UpdateProcessEvent(currentProcessInfo[_listIdx]);
                        ++_listIdx;
                    }
                }
                else
                {
                    if (CloseProcessEvent!=null)
                        CloseProcessEvent(prc);
                }
            }

            _lists = currentProcessInfo;
        }

        private void UpdateNewProcesses(System.Diagnostics.Process[] currentProcesses)
        {
            foreach (System.Diagnostics.Process prc in currentProcesses)
            {
                if (prc.Id == 0)
                    continue;
                ProcessInfo pi = Array.Find(_lists, (ProcessInfo p) => prc.Id == p.pid);

                if (pi.pid == 0)
                {
                    if (UpdateProcessInfo(ref _lists[_listIdx], prc, default(ProcessInfo)))
                    {
                        if (NewProcessEvent != null)
                            NewProcessEvent(_lists[_listIdx]);
                        ++_listIdx;
                    }
                }
            }
        }

        private bool UpdateProcessInfo(ref ProcessInfo pi, System.Diagnostics.Process prc, ProcessInfo old)
        {
            try
            {
                pi.name = prc.ProcessName;
                pi.pid = prc.Id;
                pi.mem = prc.WorkingSet64 / 1024;
                pi.cpu = (int)GetProcessCpuUsage(prc.TotalProcessorTime, old.totalProcessorTime);
                pi.threadCount = prc.Threads.Count;
                pi.filePath = prc.MainModule.FileName;
                pi.totalProcessorTime = prc.TotalProcessorTime;
                pi.startTime = prc.StartTime;
                pi.handleCount = prc.HandleCount;

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
