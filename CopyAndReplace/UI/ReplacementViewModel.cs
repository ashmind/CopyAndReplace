using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CopyAndReplace.UI {
    public class ReplacementViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = null;
        private string pattern;
        private string replacement;

        public string Pattern {
            get { return this.pattern; }
            set { Set("Pattern", ref this.pattern, value); }
        }

        public string Replacement {
            get { return this.replacement; }
            set { Set("Replacement", ref this.replacement, value); }
        }

        protected void Set<T>(string propertyName, ref T field, T value) {
            if (Equals(field, value))
                return;

            field = value;
            this.RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
