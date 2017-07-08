using NetScrattinoWin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScrattinoWin.ViewModels
{
    class MainViewModel : ObservableObject
    {
        private PinModel pinModel;
        private List<PinViewModel> items;

        public MainViewModel(PinModel model)
        {
            this.pinModel = model;

            items = new List<PinViewModel>();
            for ( int i=0; i<=13; i++ )
            {
                items.Add(new PinViewModel(model, i));
            }
            for (int i = 14; i < 14+6; i++)
            {
                items.Add(new PinViewModel(model, i, PinViewModel.MODE.ANALOG));
            }
        }

        /// <summary>
        /// List of PinViewModel
        /// </summary>
        public List<PinViewModel> Items
        {
            get { return items; }
        }

        List<string> serialItems;
        public List<string> SerialItems
        {
            get { return serialItems; }
            set { SetProperty(ref serialItems, value, nameof(SerialItems)); }
        }
        public int SerialSelectedIndex { get; set; }
        public string SerialName
        {
            get {
                if (SerialItems == null || SerialSelectedIndex < 0)
                    return "";

                return SerialItems[SerialSelectedIndex];
            }
        }

        public void Update()
        {
            foreach ( var it in items )
            {
                it.Update();
            }
        }
    }
}
