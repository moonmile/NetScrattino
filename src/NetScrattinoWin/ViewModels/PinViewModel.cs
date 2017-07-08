using NetScrattinoWin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetScrattinoWin.ViewModels
{
    class PinViewModel : ObservableObject
    {
        private PinModel pinModel;
        private int pin;
        private int pinModeIndex = 1;       // default: MODE.OUTPUT


        public PinViewModel(PinModel model, int pinNo, MODE mode = MODE.OUTPUT)
        {
            this.pinModel = model;
            this.pin = pinNo;
            this.PinMode = mode;
        }

        public enum MODE
        {
            INPUT = 0,
            OUTPUT = 1,
            ANALOG = 2,
            PWM = 3,
            SERVO = 4,
            INPUT_PULLUP = 11
        };

        public string PinName {
            get {
                if ( pin < 14 ) 
                    return "D" + pin.ToString();
                else
                    return "A" + (pin-14).ToString();
            }
        }

        public MODE PinMode {
            get
            {
                switch (pinModeIndex)
                {
                    case 0: return MODE.INPUT;
                    case 1: return MODE.OUTPUT;
                    case 2: return MODE.ANALOG;
                    case 3: return MODE.PWM;
                    case 4: return MODE.SERVO;
                    case 5: return MODE.INPUT_PULLUP;
                }
                return MODE.OUTPUT;
            }
            set
            {
                switch (value)
                {
                    case MODE.INPUT: this.PinModeIndex = 0; break;
                    case MODE.OUTPUT: this.PinModeIndex = 1; break;
                    case MODE.ANALOG: this.PinModeIndex = 2; break;
                    case MODE.PWM: this.PinModeIndex = 3; break;
                    case MODE.SERVO: this.PinModeIndex = 4; break;
                    case MODE.INPUT_PULLUP: this.PinModeIndex = 5; break;
                }
            }
        }
        public int PinModeIndex {
            get { return pinModeIndex; }
            set {
                pinModeIndex = value;
                this.pinModel.SetPinMode(pin, (int)this.PinMode);
                SetProperty(ref pinModeIndex, value, nameof(PinModeIndex));
                OnPropertyChanged(nameof(DigitalInputValueVisible));
                OnPropertyChanged(nameof(DigitalOutputValueVisible));
                OnPropertyChanged(nameof(AnalogInputValueVisible));
                OnPropertyChanged(nameof(PwmValueVisible));
                OnPropertyChanged(nameof(ServoValueVisible));
            }
        }

        public bool DigitalData { get; set; }
        public int AnalogData { get; set; }

        // 以下は,画面用
        public List<string> PinModeItems
        {
            get
            {
                return new List<string> {
                    "INPUT", "OUTPUT", "ANALOG", "PWM", "SERVO", "INPUT PULLUP"
                };
            }
        }

        public Visibility DigitalInputValueVisible {
            get { return ( PinMode == MODE.INPUT || PinMode == MODE.INPUT_PULLUP) ? Visibility.Visible : Visibility.Hidden;  }
        }
        public Visibility DigitalOutputValueVisible {
            get { return PinMode == MODE.OUTPUT ? Visibility.Visible : Visibility.Hidden; }
        }
        public Visibility AnalogInputValueVisible {
            get { return PinMode == MODE.ANALOG ? Visibility.Visible : Visibility.Hidden; }
        }
        public Visibility PwmValueVisible {
            get { return PinMode == MODE.PWM ? Visibility.Visible : Visibility.Hidden; }
        }
        public Visibility ServoValueVisible {
            get { return PinMode == MODE.SERVO ? Visibility.Visible : Visibility.Hidden; }
        }

        public string DigitalInputValueText
        {
            get
            {
                return pinModel.GetDigitalValue(this.pin) ? "High" : "Low";
            }
        }
        public bool DigitalOutputValue
        {
            get { return pinModel.GetDigitalValue(this.pin); }
            set
            {
                pinModel.SetDigitalValue(pin, value);
                OnPropertyChanged(nameof(DigitalOutputValueText));
            }
        }
        public string DigitalOutputValueText
        {
            get { return DigitalOutputValue ? "High" : "Low"; }
        }

        public string AnalogInputValueText
        {
            get
            {
                return pinModel.GetAnaloglValue(this.pin).ToString();
            }
        }

        private int analogValue;
        public int PwmValue
        {
            get {
                return analogValue;
            }
            set
            {
                pinModel.SetPinMode(pin, (int)MODE.PWM);
                pinModel.SetAnaloglValue(pin, value);
                analogValue = value;
            }
        }
        public int ServoValue
        {
            get {
                return analogValue;
            }
            set
            {
                pinModel.SetPinMode(pin, (int)MODE.SERVO);
                pinModel.SetAnaloglValue(pin, value);
                analogValue = value;
            }
        }

        public void Update()
        {
            switch(PinMode)
            {
                case MODE.INPUT:
                    OnPropertyChanged(nameof(DigitalInputValueText));
                    break;
                case MODE.INPUT_PULLUP:
                    OnPropertyChanged(nameof(DigitalInputValueText));
                    break;
                case MODE.ANALOG:
                    OnPropertyChanged(nameof(AnalogInputValueText));
                    break;
            }
        }
    }
}
