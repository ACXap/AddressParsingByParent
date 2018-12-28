using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AddressParsingByParent
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly string _name = @"c:\app\AddressElement3ALL.csv";
        private readonly string _house = @"c:\app\ОрловскаяДома.csv";
        private readonly string _houseAdr = @"c:\app\Разбор адреса\ОрловскаяДома.csv";
        private readonly string _errorAddress = @"c:\app\Разбор адреса\errorОрловскаяДома.csv";
        private readonly string _errorHouse = @"c:\app\Разбор адреса\errorHouseОрловскаяДома.csv";

        private Dictionary<int, Address> _dicAddress { get; set; }
        private Dictionary<int, List<Address>> _dic { get; set; }
        private List<House> _listHouse { get; set; }

        private List<ErrorAddress> _listErrorAddress { get; set; }
        private List<ErrorHouse> _listErrorHouse { get; set; }

        private RelayCommand _myCommand;
        public RelayCommand MyCommand
        {
            get
            {
                return _myCommand
                    ?? (_myCommand = new RelayCommand(
                    () =>
                    {
                        GetAdr();
                    }));
            }
        }

        private RelayCommand _myCommand1;
        public RelayCommand MyCommand1
        {
            get
            {
                return _myCommand1
                    ?? (_myCommand1 = new RelayCommand(
                    () =>
                    {
                        GetAdrHouse();
                    }));
            }
        }

        private RelayCommand _myCommand2;
        public RelayCommand MyCommand2
        {
            get
            {
                return _myCommand2
                    ?? (_myCommand2 = new RelayCommand(
                    () =>
                    {
                        SaveToFile();
                    }));
            }
        }

        private void SaveToFile()
        {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(_houseAdr)))
            {
                sw.WriteLine("Индекс;Область;Район;Город/поселение;Улица;Дом;Полный адрес");
                foreach (var item in _listHouse)
                {
                    sw.WriteLine($"{item.PosatalCode};{item.Region};{item.District};{item.City};{item.Street};{item.NumberHouse};{item.Address}");
                }
            }

            if(_listErrorAddress.Any())
            {
                using (StreamWriter sw = new StreamWriter(File.OpenWrite(_errorAddress)))
                {
                    sw.WriteLine("Адрес;Что не так");
                    foreach (var item in _listErrorAddress)
                    {
                        sw.WriteLine($"{item.FullAddress};{item.Why}");
                    }
                }
            }
            
            if(_listErrorHouse.Any())
            {
                using (StreamWriter sw = new StreamWriter(File.OpenWrite(_errorHouse)))
                {
                    sw.WriteLine("ГлобалID;Адрес;Что не так");
                    foreach (var item in _listErrorHouse)
                    {
                        sw.WriteLine($"{item.Address};{item.Why}");
                    }
                }
            }
        }

        private void GetAdrHouse()
        {
            int count = File.ReadAllLines(_house).Length - 1;
            _listHouse = new List<House>(count);

           // var err = 0;

            using (StreamReader sr = new StreamReader(File.Open(_house, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.Default))
            {
                var i = 0;
                while (!sr.EndOfStream)
                {
                    if (i == 0)
                    {
                        sr.ReadLine();
                        i++;
                        continue;
                    }
                    var s = sr.ReadLine();
                    var str = s.Split(';');

                    try
                    {
                        var house = new House()
                        {
                            GlobalID = int.Parse(str[0]),
                            Address = str[1],
                            PosatalCode = GetPostalCode(str[1]),
                            ParentID = int.Parse(str[2]),
                            NumberHouse = str[3],
                        };
                        GetRegionCityStreet(house);
                        _listHouse.Add(house);
                    }
                    catch (Exception ex)
                    {
                        _listErrorHouse.Add(new ErrorHouse()
                        {
                            Address = s
                        });
                      //  err++;
                    }
                }
            }
        }

        private void GetRegionCityStreet(House house)
        {
            List<Address> listAdr = new List<Address>();
            if (_dic.TryGetValue(house.ParentID, out listAdr))
            {
                var r = listAdr.FirstOrDefault(p => p.Aolevel == 1);
                if (r != null)
                {
                    house.Region = r.Name;
                }
                var d = listAdr.FirstOrDefault(p => p.Aolevel == 3);
                if (d != null)
                {
                    house.District = d.Name;
                }
                var s = listAdr.FirstOrDefault();
                if (s != null)
                {
                    if (s.Aolevel >= 7)
                    {
                        house.Street = $"{s.Name} {s.ShotName}";
                    }
                }
                var strB = new StringBuilder();
                foreach (var item in listAdr)
                {
                    if (item.Aolevel < 7 && item.Aolevel > 3)
                    {
                        strB.Insert(0, $"{item.Name} {item.ShotName},");
                    }
                }
                house.City = strB.ToString();

                if (string.IsNullOrEmpty(house.City))
                {
                    _listErrorHouse.Add(new ErrorHouse()
                    {
                        Address = $"{house.GlobalID};{house.Address}",
                        Why = "Нет города или отсутсвует родитель (скорее всего МРФ не тот)"
                    });
                }
                else
                {
                    house.City = house.City.Remove(house.City.Length - 1, 1);
                }

                strB.Clear();
                strB = null;
            }
        }

        private int GetPostalCode(string adr)
        {
            var s = adr.Split(',');

            int.TryParse(s[0], out int postalcode);

            return postalcode;
        }

        private void GetAdr()
        {
            int count = File.ReadAllLines(_name).Length - 1;
            _dicAddress = new Dictionary<int, Address>(count);
            _dic = new Dictionary<int, List<Address>>(count);

           // var err = 0;

            using (StreamReader sr = new StreamReader(File.Open(_name, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var i = 0;
                while (!sr.EndOfStream)
                {
                    if (i == 0)
                    {
                        sr.ReadLine();
                        i++;
                        continue;
                    }
                    var s = sr.ReadLine();
                    var str = s.Split(',');

                    try
                    {
                        var adr = new Address()
                        {
                            Name = str[1],
                            ShotName = str[2],
                            Aolevel = int.Parse(str[4])
                        };
                        if (int.TryParse(str[3], out int id))
                        {
                            adr.ParId = id;
                        }
                        else
                        {
                            _listErrorAddress.Add(new ErrorAddress()
                            {
                                FullAddress = s,
                                Why = "Нет родителя"
                            });
                        }
                        _dicAddress.Add(int.Parse(str[0]), adr);
                    }
                    catch (Exception ex)
                    {
                        //err++;
                        _listErrorAddress.Add(new ErrorAddress()
                        {
                            FullAddress = s
                        });
                    }
                }
            }

            foreach (var item in _dicAddress)
            {
                var isFindParent = true;

                var v = item.Value;
                _dic.Add(item.Key, new List<Address>()
                {
                    item.Value
                });

                var par = v.ParId;
                Address adr;
                while (isFindParent)
                {
                    if (_dicAddress.TryGetValue(par, out adr))
                    {
                        _dic[item.Key].Add(adr);

                        par = adr.ParId;
                    }
                    else
                    {
                        isFindParent = false;
                    }
                }
            }

            //JsonSerializerSettings settings = new JsonSerializerSettings
            //{
            //    Formatting = Formatting.Indented,
            //    ContractResolver = new DictionaryAsArrayResolver()
            //};
            //File.WriteAllText(@"d:\movie.json", JsonConvert.SerializeObject(_dic, settings));

        }

        public MainWindowViewModel()
        {
            _listErrorAddress = new List<ErrorAddress>();
            _listErrorHouse = new List<ErrorHouse>();
        }
    }
}