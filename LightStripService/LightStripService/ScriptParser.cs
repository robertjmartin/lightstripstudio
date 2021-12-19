namespace ChristmasLightServer
{
   
    public class ScriptParser
    {
        private delegate void DelayDelegate(UInt16 x);
        private delegate void SetColorDelegate(UInt16 id, byte[] rgbValues);
        private delegate byte[] RgbColorDelegate(byte red, byte green, byte blue);
        private delegate byte[] HsvColorDelegate(UInt16 hue, UInt16 saturation, UInt16 value);
        private delegate void logDelegate(string s);

        private static int cmdCount = 50000;
        private List<ICommand> _commands;
        private Jint.Engine _engine;
        private Thread _scriptThread;
        private string _script;
        private readonly EventWaitHandle _scriptPauseEvent;
        private readonly EventWaitHandle _scriptResumeEvent;
        private bool _moreWork;
        private bool _faulted;
        private ILogger _logger;

        public ScriptParser(ILogger logger)
        {
            _logger = logger;
            _engine = new Jint.Engine(options => { });
            _engine.SetValue("delay", new DelayDelegate(Delay));
            _engine.SetValue("setColor", new SetColorDelegate(SetColor));
            _engine.SetValue("rgbColor", new RgbColorDelegate(RgbColor));
            _engine.SetValue("hsvColor", new HsvColorDelegate(HsvColor));
            _engine.SetValue("log", new logDelegate(log));

            _script = string.Empty;
            _scriptPauseEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _scriptResumeEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _scriptThread = new Thread(new ThreadStart(this.scriptThread));
            _moreWork = false;
            _faulted = false;
        }

        public void StartScript(string script)
        {
            _script = script;
            _moreWork = true;
            _scriptThread.Start();
        }

        public IEnumerable<ICommand> GetCommandsFromScript()
        {
            if (!_moreWork)
            {
                return Enumerable.Empty<ICommand>();
            }

            _commands = new List<ICommand>();

            _scriptResumeEvent.Set();
            _scriptPauseEvent.WaitOne();

            if (_faulted)
            {
                return new List<ICommand>();
            }
            return _commands;
        }

        public void scriptThread()
        {
            if (_script == string.Empty)
            {
                throw new ArgumentException("_script");
            }

            _scriptResumeEvent.WaitOne();

            try
            {
                _engine.Execute(_script);
                _moreWork = false;
                _commands.Add(new DelayCommand(0));
                _scriptPauseEvent.Set();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Script Error {ex.Message}");
                _moreWork = false;
                _faulted = true;
                _scriptPauseEvent.Set();
            }
        }

        public void Delay(UInt16 x)
        {
            _commands.Add(new DelayCommand(x));
           // _engine.ResetStatementsCount();
             checkCmdCount();
        }         

        public void log(string s)
        {
            _logger.LogInformation(s);
        }

        public void SetColor(UInt16 id, byte[] rgbValues)
        {
            _commands.Add(new SetLightColorCommand(id, new Color(rgbValues[0], rgbValues[1], rgbValues[2])));
            //_engine.ResetStatementsCount();
            checkCmdCount();
        }

        public byte[] RgbColor(byte red, byte green, byte blue)
        {
            return new byte[3] {red, green, blue};
        }

        public byte[] HsvColor(UInt16 hue, UInt16 saturation, UInt16 value)
        {
            double H = (double)(hue%360) / 360;
            double S = (double)(saturation) / 100;
            double V = (double)(value) / 100;

            var hsv = new Colorspace.ColorHSV(H, S, V);
            var rgb = new Colorspace.ColorRGB(hsv);

            byte R = (byte)(rgb.R * 255);
            byte G = (byte)(rgb.G * 255);
            byte B = (byte)(rgb.B * 255);

            return new byte[3] { R, G, B };
        }

        private void checkCmdCount()
        {
            if (_commands.Count >= cmdCount)
            {
                _moreWork = true;
                _scriptPauseEvent.Set();
                _scriptResumeEvent.WaitOne();
            }
        }
    }  
}
