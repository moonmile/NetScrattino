using NetScrattinoWin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScrattinoWin.Models
{
    public class PinModel 
    {
        public FirmataNET.Arduino Arduino { get; set; }

        public bool GetDigitalValue( int pin )
        {
            if (pin < 14)
            {
                var v = Arduino?.digitalRead(pin);
                return v.HasValue ? (v.Value != 0) : false;
            }
            else
            {
                return false;
            }
        }
        public void SetDigitalValue(int pin, bool value )
        {
            Arduino?.digitalWrite(pin, value ? 1 : 0);
        }
        public int GetAnaloglValue(int pin)
        {
            if (pin >= 14)
            {
                var v = Arduino?.analogRead(pin - 14);
                return v.HasValue ? v.Value : 0;
            }
            else
            {
                return 0;
            }
        }
        public void SetAnaloglValue(int pin, int value )
        {
            Arduino?.analogWrite(pin, value);
        }
        
        public void SetPinMode( int pin, int value )
        {
            Arduino?.pinMode(pin, value);
        }
    }
}
