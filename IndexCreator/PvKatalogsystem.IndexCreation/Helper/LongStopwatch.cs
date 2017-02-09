using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PvKatalogsystem.IndexCreation.Helper
{
    public class LongStopwatch
    {
        private DateTime _startTime;
        private DateTime _stopTime;
        private bool _isStopped = true;

        public static LongStopwatch StartNew()
        {
            var sw = new LongStopwatch();
            sw.Start();
            return sw;
        }

        public void Start()
        {
            if (_isStopped)
            {
                _startTime = DateTime.Now;
                _isStopped = false;
            }
        }

        public void Stop()
        {
            _stopTime = DateTime.Now;
            _isStopped = true;
        }

        public void Reset()
        {
            Stop();
            _startTime = _stopTime;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public long ElapsedMilliseconds
        {
            get
            {
                if (_isStopped)
                {
                    return Convert.ToInt64((_stopTime - _startTime).TotalMilliseconds);
                }
                else
                {
                    return Convert.ToInt64((DateTime.Now - _startTime).TotalMilliseconds);
                }
            }
        }

    }
}
