using BLL.Services;
using BLL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.CompilerServices;


namespace BLL.Models
{
    /// <summary>
    /// Repräsentiert ein einzelnes Bauteil.
    /// Diese Klasse enthält Eigenschaften, die von der Benutzeroberfläche
    /// gebunden werden können und führt einfache Validierungen durch.
    /// </summary>
    public class PartModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // ---------------- Backing Fields ----------------
        private string _description = string.Empty;          // automatisch berechneter Anzeigetext
        private string _partype = "Bauteilart";            // Platzhalter → soll geändert werden
        private string _holeSummary = "";                        // z.B. "Bohrungen"
        private bool _isRound = false;
        private bool _isSheetMetal = false;
        private double _length =0;
        private double _materialThickness = 0;
        private string _articleNumber = "Artikelnummer";
        private string _storageLocation = "Lagerplatz";     // Platzhalter → soll geändert werden
        private string _material = "";
        private string _imagePath = "/PL;component/Resources/Images/SEAbassConnect.JPG";
        private string _partJpgPath = "";
        private string _partFilePath = "DateiPfad";
        private string _supplier = "";
        private string _storgeUnit = "Stück";
        private string _procurementType = "Fremdbeschaffung";

        private string _baseArticleNumber = "Artikelnummer";
        private bool _isUpdatingArticleNumber;
        private string? _surface;

        // ---------------- Eigenschaften ----------------

        /// <summary>Bauteilart – wird validiert (Platzhalter "Bauteilart" → Fehler).</summary>
        public string Parttype
        {
            get => _partype;
            set
            {
                if (_partype == value) return;
                _partype = value;
                //ValidateProperty(nameof(Parttype));   // nur Platzhalter prüfen
                UpdateDescription();
            }
        }

        /// <summary>Nur falls du es in der GUI bearbeiten lässt: Lagerplatz – validiert auf Platzhalter.</summary>
        public string StorageLocation
        {
            get => _storageLocation;
            set
            {
                if (_storageLocation == value) return;
                _storageLocation = value;
                //ValidateProperty(nameof(StorageLocation)); // nur Platzhalter prüfen
            }
        }

        /// <summary>Wenn true, füge Ø{Length} in Description ein.</summary>
        public bool IsRound
        {
            get => _isRound;
            set
            {
                if (_isRound == value) return;
                _isRound = value;
                UpdateDescription();
            }
        }

        /// <summary>Länge für Ø-Anzeige.</summary>
        public double Length
        {
            get => _length;
            set
            {
                if (Math.Abs(_length - value) < double.Epsilon) return;
                _length = value;
            }
        }

        /// <summary>Wenn true, füge s={MaterialThickness} in Description ein.</summary>
        public bool IsSheetMetal
        {
            get => _isSheetMetal;
            set
            {
                if (_isSheetMetal == value) return;
                _isSheetMetal = value;
                UpdateDescription();
            }
        }

        /// <summary>Materialstärke für s-Anzeige.</summary>
        public double MaterialThickness
        {
            get => _materialThickness;
            set
            {
                if (Math.Abs(_materialThickness - value) < double.Epsilon) return;
                _materialThickness = value;

            }
        }

        /// <summary>Text nach "mit …" (z. B. "Bohrungen").</summary>
        public string Holesummary
        {
            get => _holeSummary;
            set
            {
                if (_holeSummary == value) return;
                _holeSummary = value;
            }
        }

        public string Material
        {
            get => _material;
            set
            {
                if (_material == value) return;
                _material = value;
            }
        }

        /// <summary>Artikelnr. ohne Dateiendung (z. B. "ME24-6000.par" → "ME24-6000").</summary>
        public string Articlenumber
        {
            get => _articleNumber;
             
            set
            {
                if (_articleNumber == value) return;
                if (!_isUpdatingArticleNumber)
                {
                    _baseArticleNumber = RemoveSurfaceSuffix(value, _surface);
                }

                _articleNumber = value;
                OnPropertyChanged();

                if (!_isUpdatingArticleNumber)
                {
                    UpdateArticleNumberWithSurface();
                }
            }
        }

        /// <summary>Automatisch berechnete Beschreibung (z. B. "Hülse: Ø12, s=2,5 mit Bohrungen").</summary>
        public string Description
        {
            get 
            {

     
                return _description;

            } 
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
                
            }
        }

        public string Supplier
        {
            get => _supplier;
            set
            {
                if (_supplier == value) return;
                _supplier = value;
            }
        }

        public string StorageUnit
        {
            get => _storgeUnit;
            set
            {
                if (_storgeUnit == value) return;
                _storgeUnit = value;
            }
        }

        public string ProcurementType
        {
            get => _procurementType; 
            set
            {
                if (_procurementType == value) return;
                _procurementType = value;
                OnPropertyChanged();
            }
        }

        public string? Surface
        {
            get => _surface;
            set
            {
                if (_surface == value) return;

                var previousSurface = _surface;
                _surface = value;
                _baseArticleNumber = RemoveSurfaceSuffix(_articleNumber, previousSurface);
                UpdateArticleNumberWithSurface();
                OnPropertyChanged();
            }
        }



        // Optionale weitere Properties
        public double Width { get; set; }
        public double Height { get; set; }
        public string ImagePath { get => _imagePath; set => _imagePath = value; }
        public string FilePath { get => _partFilePath; set => _partFilePath = value; }
        public string JpgFilePath { get => _partJpgPath; set => _partJpgPath = value; }
        public DataTable? BomTemplate { get; set; }
        public List<BomEntry> Bom { get; set; } = new();
        public double Mass { get; set; }


        // Standard-Implementierung von INotifyPropertyChanged
        // Benachrichtigt die GUI, dass sich eine Eigenschaft geändert hat.
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public PartModel Clone()
        {
            return (PartModel)this.MemberwiseClone();
        }

        private void UpdateDescription()
        {
            Description = new PartDescriptionBuilder().BuildDescription(this);
        }

        private void UpdateArticleNumberWithSurface()
        {
            var baseValue = string.IsNullOrWhiteSpace(_baseArticleNumber)
                ? _articleNumber
                : _baseArticleNumber;

            if (string.IsNullOrWhiteSpace(baseValue))
            {
                return;
            }

            var newValue = baseValue;

            if (!string.IsNullOrWhiteSpace(_surface))
            {
                newValue = $"{baseValue}-{_surface}";
            }

            if (_articleNumber == newValue)
            {
                return;
            }

            _isUpdatingArticleNumber = true;
            try
            {
                _articleNumber = newValue;
                OnPropertyChanged(nameof(Articlenumber));
            }
            finally
            {
                _isUpdatingArticleNumber = false;
            }
        }

        private static string RemoveSurfaceSuffix(string value, string? surface)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (string.IsNullOrWhiteSpace(surface))
            {
                return value;
            }

            var suffix = $"-{surface}";

            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? value[..^suffix.Length]
                : value;
        }



    }
}
