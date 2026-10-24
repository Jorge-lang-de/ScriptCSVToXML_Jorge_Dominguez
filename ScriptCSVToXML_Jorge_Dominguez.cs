using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ScriptCSVToXML
{
    // Hacemos una clase para guardar los datos de cada película
    public class Pelicula
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Premiere { get; set; }
        public string Runtime { get; set; }
        public string ImdbScore { get; set; }
        public string Language { get; set; }
    }

    class Program
    {
        // Hecemos rutas relativas
        static string rutaCSV = "NetflixOriginals.csv";
        static string rutaXML = "PeliculasExportadas.xml";

        static void Main(string[] args)
        {
            int opcion = 0;
            // Bucle do-while que es para mantener el menú abierto hasta que elijamos salir
            do
            {
                Console.Clear();
                Console.WriteLine("=====================================");
                Console.WriteLine("    GESTIÓN NETFLIX - CSV A XML      ");
                Console.WriteLine("=====================================");
                Console.WriteLine("1. Exportar todo el archivo a XML");
                Console.WriteLine("2. Exportar por género");
                Console.WriteLine("3. Exportar por año (a partir de un año)");
                Console.WriteLine("4. Salir");
                Console.WriteLine("=====================================");
                Console.Write("Elige una opción: ");
                // Validamos que el usuario introduzca un número correctamente
                if (int.TryParse(Console.ReadLine(), out opcion))
                {
                    // Evaluamos la opción elegida en el menú
                    switch (opcion)
                    {
                        case 1:
                            ExportarTodo();
                            break;
                        case 2:
                            ExportarPorGenero();
                            break;
                        case 3:
                            ExportarPorAnio();
                            break;
                        case 4:
                            Console.WriteLine("\n¡Saliendo del programa!");
                            break;
                        default:
                            Console.WriteLine("\nOpción no válida.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("\nIntroduce un número válido.");
                }
                // Pausamos la ejecución para poder ver el resultado antes de limpiar la pantalla
                if (opcion != 4)
                {
                    Console.WriteLine("\nPulsa cualquier tecla para continuar...");
                    Console.ReadKey();
                }

            } while (opcion != 4);
        }

        // Exportamos todo el CSV a XML
        static void ExportarTodo()
        {
            // Comprobamos que el archivo CSV exista antes de que lo intente leerlo
            if (!File.Exists(rutaCSV))
            {
                Console.WriteLine("No se encuentra el archivo NetflixOriginals.csv");
                return;
            }

            List<Pelicula> peliculas = LeerCsv(rutaCSV);
            GenerarXml(peliculas, rutaXML);

            Console.WriteLine($"\n¡Listo! Se han exportado {peliculas.Count} películas a '{rutaXML}'.");
        }

        // Exportamos las peliculas filtrando por género
        static void ExportarPorGenero()
        {
            if (!File.Exists(rutaCSV)) return;

            List<Pelicula> peliculas = LeerCsv(rutaCSV);

            Console.Write("\nIntroduce el género que quieres buscar (ej. Documentary, Comedy): ");
            string generoBuscado = Console.ReadLine().Trim();
            // filtamos la lista por si escribimos mayúsculas o minúsculas no nos de error y podamos escribirlo como queramos 
            var filtradas = peliculas.Where(p => p.Genre.Equals(generoBuscado, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filtradas.Count > 0)
            {
                string archivoSalida = $"Netflix_{generoBuscado}.xml";
                GenerarXml(filtradas, archivoSalida);
                Console.WriteLine($"\n¡Éxito! Se han guardado {filtradas.Count} películas de '{generoBuscado}' en '{archivoSalida}'.");
            }
            else
            {
                Console.WriteLine("\nNo se han encontrado películas con ese género.");
            }
        }

        // Exportamos las peliculas filtrando por año mínimo
        static void ExportarPorAnio()
        {
            if (!File.Exists(rutaCSV)) return;

            List<Pelicula> peliculas = LeerCsv(rutaCSV);

            Console.Write("\nIntroduce el año mínimo (ej. 2020): ");
            if (int.TryParse(Console.ReadLine(), out int anioMin))
            {
                // Buscamos si la fecha de estreno contiene el año introducido
                var filtradas = peliculas.Where(p => p.Premiere.Contains(anioMin.ToString())).ToList();

                string archivoSalida = $"Netflix_Desde_{anioMin}.xml";
                GenerarXml(filtradas, archivoSalida);
                Console.WriteLine($"\n¡Éxito! Se han guardado {filtradas.Count} películas a partir del año {anioMin}.");
            }
            else
            {
                Console.WriteLine("\nAño no válido.");
            }
        }

        // utilizamos un método auxiliar para leer el CSV respetando las comas de los títulos
        static List<Pelicula> LeerCsv(string ruta)
        {
            List<Pelicula> lista = new List<Pelicula>();
            string[] lineas = File.ReadAllLines(ruta);

            // Empezamos en 1 para saltar la cabecera
            for (int i = 1; i < lineas.Length; i++)
            {
                string linea = lineas[i];
                if (string.IsNullOrWhiteSpace(linea)) continue;

                // Esto evita que las comas de dentro de los títulos rompan las columnas
                string[] campos = System.Text.RegularExpressions.Regex.Split(linea, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                // Comprobamos que la línea tenga al menos 6 campos para evitar errores de índice
                if (campos.Length >= 6)
                {
                    // Creamos un nuevo objeto Pelicula y mapeamos cada columna del CSV
                    Pelicula p = new Pelicula
                    {
                        // se limpian las comillas y los espacios de todos los campos para que no nos de error
                        Title = campos[0].Trim('"').Trim(),
                        Genre = campos[1].Trim('"').Trim(),
                        Premiere = campos[2].Trim('"').Trim(),
                        Runtime = campos[3].Trim('"').Trim(),
                        ImdbScore = campos[4].Trim('"').Trim(),
                        Language = campos[5].Trim('"').Trim()
                    };
                    lista.Add(p);
                }
            }

            return lista;
        }

        // utilizamos un método auxiliar para crear el XML con la estructura pedida
        static void GenerarXml(List<Pelicula> peliculas, string nombreFichero)
        {
            XDocument doc = new XDocument(
                new XElement("Peliculas",
                    // Usamos .Select para que recorra cada objeto Pelicula 
                    peliculas.Select(p => new XElement("Pelicula",
                        new XElement("Title", p.Title),
                        new XElement("Genre", p.Genre),
                        new XElement("Premiere", p.Premiere),
                        new XElement("Runtime", p.Runtime),
                        new XElement("IMDBScore", p.ImdbScore),
                        new XElement("Language", p.Language)
                    ))
                )
            );
            // Guardamos el documento XML
            doc.Save(nombreFichero);
        }
    }
}

