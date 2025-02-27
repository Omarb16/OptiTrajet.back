﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OptiTrajet.Domain.Entities;
using OptiTrajet.Persistence;
using OptiTrajet.Populate.Model;
using Serilog;

namespace OptiTrajet.Populate.Services
{
    public class PopulateDb : IPopulateDb
    {
        private readonly ILogger _logger;
        private readonly OptiTrajetContext _dbContext;
        private readonly Line[] lines = new Line[43]
        {
            new Line(){ Name= "METRO 1", Color= "#fece00" },
            new Line(){ Name= "METRO 2", Color="#0065ae" },
            new Line(){ Name= "METRO 3", Color="#9f971a" },
            new Line(){ Name= "METRO 3bis", Color="#89D3DE" },
            new Line(){ Name= "METRO 4", Color= "#be418d" },
            new Line(){ Name= "METRO 5", Color= "#f19043" },
            new Line(){ Name= "METRO 6", Color= "#77c695" },
            new Line(){ Name= "METRO 7", Color= "#f2a4b7" },
            new Line(){ Name= "METRO 7bis", Color= "#77C697" },
            new Line(){ Name= "METRO 8", Color= "#cdaccf" },
            new Line(){ Name= "METRO 9", Color= "#d5c900" },
            new Line(){ Name= "METRO 10", Color= "#e4b327" },
            new Line(){ Name= "METRO 11", Color= "#8c5e24" },
            new Line(){ Name= "METRO 12", Color= "#007e49" },
            new Line(){ Name= "METRO 13", Color= "#99d4de" },
            new Line(){ Name= "METRO 14", Color= "#622280" },
            new Line(){ Name= "RER A", Color= "#ff1400" },
            new Line(){ Name= "RER B", Color= "#3c91dc" },
            new Line(){ Name= "RER C", Color= "#ffbe00" },
            new Line(){ Name= "RER D", Color= "#00643c" },
            new Line(){ Name= "RER E", Color= "#a0006e" },
            new Line(){ Name= "TRAIN H", Color= "#6e491e" },
            new Line(){ Name= "TRAIN J", Color= "#d2d200" },
            new Line(){ Name= "TRAIN K", Color= "#6e6e00" },
            new Line(){ Name= "TRAIN L", Color= "#d282be" },
            new Line(){ Name= "TRAIN N", Color= "#00a092" },
            new Line(){ Name= "TRAIN P", Color= "#ff5a00" },
            new Line(){ Name= "TRAIN R", Color= "#ff82b4" },
            new Line(){ Name= "TRAIN U", Color= "#a50034" },
            new Line(){ Name= "TRAM 1", Color= "#0055c8" },
            new Line(){ Name= "TRAM 2", Color= "#a0006e" },
            new Line(){ Name= "TRAM 3a", Color= "#ff5a00" },
            new Line(){ Name= "TRAM 3b", Color= "#00643c" },
            new Line(){ Name= "TRAM 4", Color= "#dc9600" },
            new Line(){ Name= "TRAM 5", Color= "#640082" },
            new Line(){ Name= "TRAM 6", Color= "#ff0000" },
            new Line(){ Name= "TRAM 7", Color= "#6e491e" },
            new Line(){ Name= "TRAM 8", Color= "#6e6e00" },
            new Line(){ Name= "TRAM 9", Color= "#3c91dc" },
            new Line(){ Name= "TRAM 10", Color= "#C4D887" },
            new Line(){ Name= "TRAM 12", Color= "#AB0333" },
            new Line(){ Name= "TRAM 11", Color= "#ff5a00" },
            new Line(){ Name= "TRAM 13", Color= "#6e491e" }
        };

        public PopulateDb(ILogger logger, OptiTrajetContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Populate()
        {
            _logger.Information("Began");

            var stationsJson = GetStationsFromJson();

            var citiesJson = GetCitiesFromJson();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Itineraries");
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Stations");
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Cities");
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Lines");
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Places");

                    await _dbContext.Lines.AddRangeAsync(lines);

                    var cities = citiesJson.Select(s => new Domain.Entities.City
                    {
                        Name = s.name,
                        Coordianates = JsonConvert.SerializeObject(s.coordinates),
                    }).ToArray();

                    await _dbContext.Cities.AddRangeAsync(cities);

                    var dlines = lines.ToDictionary(x => x.Name);

                    var stations = stationsJson
                    .Where(x => !new List<string> { "FUNICULAIRE MONTMARTRE", "GL", "CDGVAL", "ORLYVAL" }.Contains(x.Line))
                    .Select(s =>
                    {
                        var cityId = cities.Where(b =>
                        {
                            var x = s.Position.Lat;
                            var y = s.Position.Lng;

                            var inside = false;

                            var coordianates = JsonConvert.DeserializeObject<decimal[][]>(b.Coordianates)!;

                            for (var i = 0; i < coordianates.Length; i++)
                            {
                                var j = (i + coordianates.Length - 1) % coordianates.Length;
                                var xi = coordianates[i][0];
                                var yi = coordianates[i][1];
                                var xj = coordianates[j][0];
                                var yj = coordianates[j][1];

                                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                                if (intersect) inside = !inside;
                            }
                            return inside;
                        })
                        .Select(x => x.Id)
                        .FirstOrDefault();

                        return new Domain.Entities.Station
                        {
                            Lat = s.Position.Lat,
                            Lon = s.Position.Lng,
                            LineId = dlines[s.Line].Id,
                            Name = s.Name,
                            CityId = cityId != Guid.Empty ? cityId : null,
                        };
                    }).ToArray();

                    await _dbContext.Stations.AddRangeAsync(stations);

                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.Error("{ex}", ex);
                }
            }

            _logger.Information("Done !");
        }

        private Model.Station[] GetStationsFromJson()
        {
            using StreamReader streamReaderS = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv2\emplacement-des-gares-idf.json"));
            string jsonS = streamReaderS.ReadToEnd();

            return JsonConvert.DeserializeObject<Model.Station[]>(jsonS)!;
        }

        private Model.City[] GetCitiesFromJson()
        {
            using StreamReader streamReaderR = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv2\zones_idf.json"));
            string jsonR = streamReaderR.ReadToEnd();

            return JsonConvert.DeserializeObject<Model.City[]>(jsonR)!;
        }

        public void ModifiedStations()
        {
            using StreamReader r = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v2\emplacement-des-gares-idf.json"));
            string json = r.ReadToEnd();
            var data = JsonConvert.DeserializeObject<List<StationIDFMobilites>>(json);

            var d = data.Select(s => new Model.Station
            {
                Position = new Position
                {
                    Lat = s.geo_point_2d.lat,
                    Lng = s.geo_point_2d.lon,
                },
                Name = s.nom_gares,
                Line = s.res_com,
            }).ToList();

            File.WriteAllText(@"Files\modifiedv2\emplacement-des-gares-idf.json", JsonConvert.SerializeObject(d));
        }

        public void ModifiedCities()
        {
            using StreamReader r = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v2\zones_idf.json"));
            string json = r.ReadToEnd();
            var data = JsonConvert.DeserializeObject<CityIDF>(json);

            var d = data.features.Select(s => new Model.City
            {
                name = s.properties.nom,
                coordinates = s.geometry.coordinates[0].Select(s => s.Reverse().ToArray()).ToArray(),
            }).ToList();

            File.WriteAllText(@"Files\modifiedv2\zones_idf.json", JsonConvert.SerializeObject(d));
        }

        public void Count()
        {
            //using StreamReader r1 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v1\zones_idf.json"));
            //string json = r1.ReadToEnd();
            //var data1 = JsonConvert.DeserializeObject<CityIDF>(json);
            //Console.WriteLine("v1 zones_idf : " + data1.features.Count);

            using StreamReader r2 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v2\zones_idf.json"));
            var json = r2.ReadToEnd();
            var data1 = JsonConvert.DeserializeObject<CityIDF>(json);
            Console.WriteLine("v2 zones_idf : " + data1.features.Count);

            //using StreamReader r3 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv1\zones_idf.json"));
            //json = r3.ReadToEnd();
            //var data2 = JsonConvert.DeserializeObject<Model.City[]>(json);
            //Console.WriteLine("modifiedv1 zones_idf : " + data2.Length);

            using StreamReader r4 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv2\zones_idf.json"));
            json = r4.ReadToEnd();
            var data2 = JsonConvert.DeserializeObject<Model.City[]>(json);
            Console.WriteLine("modifiedv2 zones_idf : " + data2.Length);

            //using StreamReader r5 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v1\emplacement-des-gares-idf.json"));
            //json = r5.ReadToEnd();
            //var data3 = JsonConvert.DeserializeObject<StationIDFMobilites[]>(json);
            //Console.WriteLine("v1 emplacement-des-gares-idf : " + data3.Length);

            using StreamReader r6 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\v2\emplacement-des-gares-idf.json"));
            json = r6.ReadToEnd();
            var data3 = JsonConvert.DeserializeObject<StationIDFMobilites[]>(json);
            Console.WriteLine("v2 emplacement-des-gares-idf : " + data3.Length);

            //using StreamReader r7 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv1\emplacement-des-gares-idf.json"));
            //json = r7.ReadToEnd();
            //var data4 = JsonConvert.DeserializeObject<Model.Station[]>(json);
            //Console.WriteLine("modifiedv1 emplacement-des-gares-idf : " + data4.Length);

            using StreamReader r8 = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"Files\modifiedv2\emplacement-des-gares-idf.json"));
            json = r8.ReadToEnd();
            var data4 = JsonConvert.DeserializeObject<Model.Station[]>(json);
            Console.WriteLine("modifiedv2 emplacement-des-gares-idf : " + data4.Length);
        }
    }
}
