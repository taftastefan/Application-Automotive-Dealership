using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace DealershipApp.Controllers
{
    public class HomeController : Controller
    {
        private TinyDB tinyDB;
       private string connectionString = "Server=DESKTOP-9B089QR\\SQLEXPRESS;Database=AplicatieEvidentaVanzareMasini;Trusted_Connection=True;";



        public HomeController()
        {
            tinyDB = new TinyDB();
        }

        // Pagina de start (Index) pentru autentificare
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (tinyDB.VerifyUser(username, password))
            {
                return RedirectToAction("MainMenu");
            }
            else
            {
                ViewBag.Error = "Invalid username or password.";
                return View("Index");
            }
        }

        // Pagina pentru înregistrare
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            tinyDB.AddUser(username, password);
            return RedirectToAction("Index");
        }

        // Meniul principal după autentificare
        [HttpGet]
        public IActionResult MainMenu()
        {
            return View();
        }

        // Afișează toate mașinile din baza de date MySQL
        [HttpGet]
public IActionResult ViewCars()
{
    var cars = new List<Car>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT 
                m.IDMasina, 
                m.An_fabricatie, 
                m.Kilometraj, 
                m.Pret, 
                m.Stare, 
                md.Denumire AS Model, 
                ma.Denumire AS Marca
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            cars.Add(new Car
            {
                IDMasina = reader.GetInt32(0),
                AnFabricatie = reader.GetInt32(1),
                Kilometraj = reader.GetInt32(2),
                Pret = reader.GetString(3),
                Stare = reader.GetString(4),
                Model = reader.GetString(5),
                Marca = reader.GetString(6)
            });
        }
        conn.Close();
    }
    return View(cars);
}


        // Afișează toți clienții din baza de date MySQL
        [HttpGet]
        public IActionResult ViewClients()
        {
            var clients = new List<Client>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT IDClient, Nume, Prenume, Email, Telefon FROM Client";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        IDClient = reader.GetInt32(0),
                        Nume = reader.GetString(1),
                        Prenume = reader.GetString(2),
                        Email = reader.GetString(3),
                        Telefon = reader.GetString(4)
                    });
                }
                conn.Close();
            }
            return View(clients);
        }

        // Afișează toate vânzările din baza de date MySQL
        [HttpGet]
public IActionResult ViewSales()
{
    var sales = new List<Sale>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT 
                v.IDVanzare, 
                v.Data_vanzare, 
                v.Pret_final, 
                c.Nume + ' ' + c.Prenume AS Client, 
                ma.Denumire AS Marca, 
                md.Denumire AS Model, 
                a.Nume + ' ' + a.Prenume AS Angajat
            FROM Vanzare v
            INNER JOIN Client c ON v.IDClient = c.IDClient
            INNER JOIN Masina m ON v.IDMasina = m.IDMasina
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            INNER JOIN Angajat a ON v.IDAngajat = a.IDAngajat";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            sales.Add(new Sale
            {
                IDVanzare = reader.GetInt32(0),
                DataVanzare = reader.GetDateTime(1),
                PretFinal = reader.GetString(2),
                Client = reader.GetString(3),
                Marca = reader.GetString(4),
                Model = reader.GetString(5),
                Angajat = reader.GetString(6)
            });
        }
        conn.Close();
    }
    return View(sales);
}
    [HttpGet]
public IActionResult AddClient()
{
    return View();
}

[HttpPost]
public IActionResult AddClient(Client client)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "INSERT INTO Client (Nume, Prenume, Email, Telefon) VALUES (@Nume, @Prenume, @Email, @Telefon)";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nume", client.Nume);
        cmd.Parameters.AddWithValue("@Prenume", client.Prenume);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telefon", client.Telefon);
        conn.Open();
        cmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewClients");
}
  [HttpGet]
public IActionResult AddCar()
{
    var models = new List<Model>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "SELECT IDModel, Denumire FROM Model";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            models.Add(new Model
            {
                IDModel = reader.GetInt32(0),
                Denumire = reader.GetString(1)
            });
        }
        conn.Close();
    }
    ViewBag.Models = models;
    return View();
}


[HttpPost]
public IActionResult AddCar(Car car, string selectedModel)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Obținem ID-ul modelului pe baza numelui selectat
        string getModelQuery = "SELECT IDModel FROM Model WHERE Denumire = @ModelName";
        SqlCommand getModelCmd = new SqlCommand(getModelQuery, conn);
        getModelCmd.Parameters.AddWithValue("@ModelName", selectedModel);
        conn.Open();
        var modelId = (int?)getModelCmd.ExecuteScalar();
        conn.Close();

        if (modelId == null)
        {
            ModelState.AddModelError("", "Modelul selectat nu există.");
            return RedirectToAction("AddCar");
        }

        // Adăugăm mașina cu ID-ul modelului
        string insertQuery = "INSERT INTO Masina (An_fabricatie, Kilometraj, Pret, Stare, IDModel) VALUES (@AnFabricatie, @Kilometraj, @Pret, @Stare, @IDModel)";
        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@AnFabricatie", car.AnFabricatie);
        insertCmd.Parameters.AddWithValue("@Kilometraj", car.Kilometraj);
        insertCmd.Parameters.AddWithValue("@Pret", car.Pret);
        insertCmd.Parameters.AddWithValue("@Stare", car.Stare);
        insertCmd.Parameters.AddWithValue("@IDModel", modelId);
        conn.Open();
        insertCmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewCars");
}


[HttpGet]
public IActionResult AddSale()
{
    var clients = new List<Client>();
    var cars = new List<Car>();
    var employees = new List<Employee>();

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Preluăm clienții
        string clientQuery = "SELECT IDClient, Nume + ' ' + Prenume AS NumeComplet FROM Client";
        SqlCommand clientCmd = new SqlCommand(clientQuery, conn);
        conn.Open();
        SqlDataReader clientReader = clientCmd.ExecuteReader();
        while (clientReader.Read())
        {
            clients.Add(new Client
            {
                IDClient = clientReader.GetInt32(0),
                Nume = clientReader.GetString(1)
            });
        }
        conn.Close();

        // Preluăm mașinile
        string carQuery = @"
            SELECT m.IDMasina, md.Denumire + ' (' + ma.Denumire + ')' AS ModelComplet
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca";
        SqlCommand carCmd = new SqlCommand(carQuery, conn);
        conn.Open();
        SqlDataReader carReader = carCmd.ExecuteReader();
        while (carReader.Read())
        {
            cars.Add(new Car
            {
                IDMasina = carReader.GetInt32(0),
                Model = carReader.GetString(1)
            });
        }
        conn.Close();

        // Preluăm angajații
        string employeeQuery = "SELECT IDAngajat, Nume + ' ' + Prenume AS NumeComplet FROM Angajat";
        SqlCommand employeeCmd = new SqlCommand(employeeQuery, conn);
        conn.Open();
        SqlDataReader employeeReader = employeeCmd.ExecuteReader();
        while (employeeReader.Read())
        {
            employees.Add(new Employee
            {
                IDAngajat = employeeReader.GetInt32(0),
                Nume = employeeReader.GetString(1)
            });
        }
        conn.Close();
    }

    ViewBag.Clients = clients;
    ViewBag.Cars = cars;
    ViewBag.Employees = employees;

    return View();
}


[HttpPost]
public IActionResult AddSale(Sale sale, string selectedClient, string selectedCar, string selectedEmployee)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Găsim ID-ul clientului
        string clientQuery = "SELECT IDClient FROM Client WHERE Nume + ' ' + Prenume = @ClientName";
        SqlCommand clientCmd = new SqlCommand(clientQuery, conn);
        clientCmd.Parameters.AddWithValue("@ClientName", selectedClient);
        conn.Open();
        var clientId = (int?)clientCmd.ExecuteScalar();
        conn.Close();

        // Găsim ID-ul mașinii
        string carQuery = @"
            SELECT m.IDMasina
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            WHERE md.Denumire + ' (' + ma.Denumire + ')' = @CarModel";
        SqlCommand carCmd = new SqlCommand(carQuery, conn);
        carCmd.Parameters.AddWithValue("@CarModel", selectedCar);
        conn.Open();
        var carId = (int?)carCmd.ExecuteScalar();
        conn.Close();

        // Găsim ID-ul angajatului
        string employeeQuery = "SELECT IDAngajat FROM Angajat WHERE Nume + ' ' + Prenume = @EmployeeName";
        SqlCommand employeeCmd = new SqlCommand(employeeQuery, conn);
        employeeCmd.Parameters.AddWithValue("@EmployeeName", selectedEmployee);
        conn.Open();
        var employeeId = (int?)employeeCmd.ExecuteScalar();
        conn.Close();

        // Verificăm dacă toate ID-urile au fost găsite
        if (clientId == null || carId == null || employeeId == null)
        {
            ModelState.AddModelError("", "Unul sau mai multe elemente selectate nu sunt valide.");
            return RedirectToAction("AddSale");
        }

        // Inserăm vânzarea în baza de date
        string insertQuery = "INSERT INTO Vanzare (Data_vanzare, Pret_final, IDClient, IDMasina, IDAngajat) VALUES (@DataVanzare, @PretFinal, @IDClient, @IDMasina, @IDAngajat)";
        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@DataVanzare", sale.DataVanzare);
        insertCmd.Parameters.AddWithValue("@PretFinal", sale.PretFinal);
        insertCmd.Parameters.AddWithValue("@IDClient", clientId);
        insertCmd.Parameters.AddWithValue("@IDMasina", carId);
        insertCmd.Parameters.AddWithValue("@IDAngajat", employeeId);
        conn.Open();
        insertCmd.ExecuteNonQuery();
        conn.Close();
    }

    return RedirectToAction("ViewSales");
}


   [HttpPost]
public IActionResult DeleteClient(int id)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "DELETE FROM Client WHERE IDClient = @ID";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        cmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewClients");
}
  [HttpPost]
public IActionResult DeleteCar(int id)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "DELETE FROM Masina WHERE IDMasina = @ID";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        cmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewCars");
}
   [HttpPost]
public IActionResult DeleteSale(int id)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "DELETE FROM Vanzare WHERE IDVanzare = @ID";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        cmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewSales");
}
   [HttpGet]
public IActionResult EditClient(int id)
{
    Client client = null;
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "SELECT IDClient, Nume, Prenume, Email, Telefon FROM Client WHERE IDClient = @ID";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            client = new Client
            {
                IDClient = reader.GetInt32(0),
                Nume = reader.GetString(1),
                Prenume = reader.GetString(2),
                Email = reader.GetString(3),
                Telefon = reader.GetString(4)
            };
        }
        conn.Close();
    }
    return View(client);
}

[HttpPost]
public IActionResult EditClient(Client client)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = "UPDATE Client SET Nume = @Nume, Prenume = @Prenume, Email = @Email, Telefon = @Telefon WHERE IDClient = @ID";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nume", client.Nume);
        cmd.Parameters.AddWithValue("@Prenume", client.Prenume);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telefon", client.Telefon);
        cmd.Parameters.AddWithValue("@ID", client.IDClient);
        conn.Open();
        cmd.ExecuteNonQuery();
        conn.Close();
    }
    return RedirectToAction("ViewClients");
}
    [HttpGet]
public IActionResult EditCar(int id)
{
    Car car = null;
    var models = new List<Model>();

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Preluăm mașina selectată
        string carQuery = "SELECT IDMasina, An_fabricatie, Kilometraj, Pret, Stare, md.Denumire AS ModelName FROM Masina m INNER JOIN Model md ON m.IDModel = md.IDModel WHERE IDMasina = @ID";
        SqlCommand carCmd = new SqlCommand(carQuery, conn);
        carCmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        SqlDataReader carReader = carCmd.ExecuteReader();
        if (carReader.Read())
        {
            car = new Car
            {
                IDMasina = carReader.GetInt32(0),
                AnFabricatie = carReader.GetInt32(1),
                Kilometraj = carReader.GetInt32(2),
                Pret = carReader.GetString(3),
                Stare = carReader.GetString(4),
                Model = carReader.GetString(5) // Numele modelului
            };
        }
        conn.Close();

        // Preluăm lista de modele
        string modelQuery = "SELECT IDModel, Denumire FROM Model";
        SqlCommand modelCmd = new SqlCommand(modelQuery, conn);
        conn.Open();
        SqlDataReader modelReader = modelCmd.ExecuteReader();
        while (modelReader.Read())
        {
            models.Add(new Model
            {
                IDModel = modelReader.GetInt32(0),
                Denumire = modelReader.GetString(1)
            });
        }
        conn.Close();
    }

    ViewBag.Models = models;
    return View(car);
}


[HttpPost]
public IActionResult EditCar(Car car, string selectedModel)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Găsim ID-ul modelului pe baza numelui selectat
        string getModelQuery = "SELECT IDModel FROM Model WHERE Denumire = @ModelName";
        SqlCommand getModelCmd = new SqlCommand(getModelQuery, conn);
        getModelCmd.Parameters.AddWithValue("@ModelName", selectedModel);
        conn.Open();
        var modelId = (int?)getModelCmd.ExecuteScalar();
        conn.Close();

        if (modelId == null)
        {
            ModelState.AddModelError("", "Modelul selectat nu există.");
            return RedirectToAction("EditCar", new { id = car.IDMasina });
        }

        // Actualizăm mașina
        string updateQuery = "UPDATE Masina SET An_fabricatie = @AnFabricatie, Kilometraj = @Kilometraj, Pret = @Pret, Stare = @Stare, IDModel = @IDModel WHERE IDMasina = @IDMasina";
        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@AnFabricatie", car.AnFabricatie);
        updateCmd.Parameters.AddWithValue("@Kilometraj", car.Kilometraj);
        updateCmd.Parameters.AddWithValue("@Pret", car.Pret);
        updateCmd.Parameters.AddWithValue("@Stare", car.Stare);
        updateCmd.Parameters.AddWithValue("@IDModel", modelId);
        updateCmd.Parameters.AddWithValue("@IDMasina", car.IDMasina);
        conn.Open();
        updateCmd.ExecuteNonQuery();
        conn.Close();
    }

    return RedirectToAction("ViewCars");
}


   [HttpGet]
public IActionResult EditSale(int id)
{
    Sale sale = null;
    var clients = new List<Client>();
    var cars = new List<Car>();
    var employees = new List<Employee>();

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Preluăm detaliile vânzării selectate
        string saleQuery = @"
            SELECT v.IDVanzare, v.Data_vanzare, v.Pret_final, 
                   c.Nume + ' ' + c.Prenume AS ClientName, 
                   md.Denumire + ' (' + ma.Denumire + ')' AS CarModel, 
                   a.Nume + ' ' + a.Prenume AS EmployeeName
            FROM Vanzare v
            INNER JOIN Client c ON v.IDClient = c.IDClient
            INNER JOIN Masina m ON v.IDMasina = m.IDMasina
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            INNER JOIN Angajat a ON v.IDAngajat = a.IDAngajat
            WHERE v.IDVanzare = @ID";
        SqlCommand saleCmd = new SqlCommand(saleQuery, conn);
        saleCmd.Parameters.AddWithValue("@ID", id);
        conn.Open();
        SqlDataReader saleReader = saleCmd.ExecuteReader();
        if (saleReader.Read())
        {
            sale = new Sale
            {
                IDVanzare = saleReader.GetInt32(0),
                DataVanzare = saleReader.GetDateTime(1),
                PretFinal = saleReader.GetString(2),
                Client = saleReader.GetString(3),
                Model = saleReader.GetString(4),
                Angajat = saleReader.GetString(5)
            };
        }
        conn.Close();

        // Preluăm lista clienților
        string clientQuery = "SELECT IDClient, Nume + ' ' + Prenume AS NumeComplet FROM Client";
        SqlCommand clientCmd = new SqlCommand(clientQuery, conn);
        conn.Open();
        SqlDataReader clientReader = clientCmd.ExecuteReader();
        while (clientReader.Read())
        {
            clients.Add(new Client
            {
                IDClient = clientReader.GetInt32(0),
                Nume = clientReader.GetString(1)
            });
        }
        conn.Close();

        // Preluăm lista mașinilor
        string carQuery = @"
            SELECT m.IDMasina, md.Denumire + ' (' + ma.Denumire + ')' AS ModelComplet
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca";
        SqlCommand carCmd = new SqlCommand(carQuery, conn);
        conn.Open();
        SqlDataReader carReader = carCmd.ExecuteReader();
        while (carReader.Read())
        {
            cars.Add(new Car
            {
                IDMasina = carReader.GetInt32(0),
                Model = carReader.GetString(1)
            });
        }
        conn.Close();

        // Preluăm lista angajaților
        string employeeQuery = "SELECT IDAngajat, Nume + ' ' + Prenume AS NumeComplet FROM Angajat";
        SqlCommand employeeCmd = new SqlCommand(employeeQuery, conn);
        conn.Open();
        SqlDataReader employeeReader = employeeCmd.ExecuteReader();
        while (employeeReader.Read())
        {
            employees.Add(new Employee
            {
                IDAngajat = employeeReader.GetInt32(0),
                Nume = employeeReader.GetString(1)
            });
        }
        conn.Close();
    }

    ViewBag.Clients = clients;
    ViewBag.Cars = cars;
    ViewBag.Employees = employees;

    return View(sale);
}


[HttpPost]
public IActionResult EditSale(Sale sale, string selectedClient, string selectedCar, string selectedEmployee)
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        // Găsim ID-ul clientului
        string clientQuery = "SELECT IDClient FROM Client WHERE Nume + ' ' + Prenume = @ClientName";
        SqlCommand clientCmd = new SqlCommand(clientQuery, conn);
        clientCmd.Parameters.AddWithValue("@ClientName", selectedClient);
        conn.Open();
        var clientId = (int?)clientCmd.ExecuteScalar();
        conn.Close();

        // Găsim ID-ul mașinii
        string carQuery = @"
            SELECT m.IDMasina
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            WHERE md.Denumire + ' (' + ma.Denumire + ')' = @CarModel";
        SqlCommand carCmd = new SqlCommand(carQuery, conn);
        carCmd.Parameters.AddWithValue("@CarModel", selectedCar);
        conn.Open();
        var carId = (int?)carCmd.ExecuteScalar();
        conn.Close();

        // Găsim ID-ul angajatului
        string employeeQuery = "SELECT IDAngajat FROM Angajat WHERE Nume + ' ' + Prenume = @EmployeeName";
        SqlCommand employeeCmd = new SqlCommand(employeeQuery, conn);
        employeeCmd.Parameters.AddWithValue("@EmployeeName", selectedEmployee);
        conn.Open();
        var employeeId = (int?)employeeCmd.ExecuteScalar();
        conn.Close();

        // Verificăm dacă toate ID-urile au fost găsite
        if (clientId == null || carId == null || employeeId == null)
        {
            ModelState.AddModelError("", "Unul sau mai multe elemente selectate nu sunt valide.");
            return RedirectToAction("EditSale", new { id = sale.IDVanzare });
        }

        // Actualizăm vânzarea
        string updateQuery = "UPDATE Vanzare SET Data_vanzare = @DataVanzare, Pret_final = @PretFinal, IDClient = @IDClient, IDMasina = @IDMasina, IDAngajat = @IDAngajat WHERE IDVanzare = @IDVanzare";
        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@DataVanzare", sale.DataVanzare);
        updateCmd.Parameters.AddWithValue("@PretFinal", sale.PretFinal);
        updateCmd.Parameters.AddWithValue("@IDClient", clientId);
        updateCmd.Parameters.AddWithValue("@IDMasina", carId);
        updateCmd.Parameters.AddWithValue("@IDAngajat", employeeId);
        updateCmd.Parameters.AddWithValue("@IDVanzare", sale.IDVanzare);
        conn.Open();
        updateCmd.ExecuteNonQuery();
        conn.Close();
    }

    return RedirectToAction("ViewSales");
}


[HttpGet]
public IActionResult Statistici(int? an)
{
    if (an.HasValue)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
                SELECT 
                    c.Nume + ' ' + c.Prenume AS Client, 
                    SUM(CAST(v.Pret_final AS DECIMAL(18,2))) AS TotalVanzari
                FROM Vanzare v
                INNER JOIN Client c ON v.IDClient = c.IDClient
                WHERE YEAR(v.Data_vanzare) = @An
                GROUP BY c.Nume, c.Prenume";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@An", an.Value);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            var clientiAn = new List<dynamic>();
            decimal totalVanzari = 0;

            while (reader.Read())
            {
                string totalAchizitiiString = reader.GetValue(1)?.ToString();
                decimal totalAchizitii = decimal.TryParse(totalAchizitiiString, out var tempVal) ? tempVal : 0;

                clientiAn.Add(new
                {
                    NumeClient = reader.GetString(0),
                    TotalAchizitii = totalAchizitii
                });
                totalVanzari += totalAchizitii;
            }

            ViewBag.ClientiAn = clientiAn;
            ViewBag.TotalVanzari = totalVanzari;
            ViewBag.An = an.Value;
            conn.Close();
        }
    }
    else
    {
        ViewBag.ClientiAn = new List<dynamic>();
        ViewBag.TotalVanzari = 0;
    }

    //  Marcă și numărul de modele cu preț mediu mai mare decât media tuturor mașinilor
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT 
                ma.Denumire AS Marca, 
                COUNT(md.IDModel) AS NrModele
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            GROUP BY ma.Denumire
            HAVING AVG(CAST(m.Pret AS DECIMAL(18,2))) > (
                SELECT AVG(CAST(mm.Pret AS DECIMAL(18,2))) 
                FROM Masina mm
            )";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        var marciModele = new List<dynamic>();
        while (reader.Read())
        {
            marciModele.Add(new
            {
                Marca = reader.GetString(0),
                NrModele = reader.GetInt32(1)
            });
        }
        ViewBag.MarciModele = marciModele;
        conn.Close();
    }

    // Mașinile cu cel mai mare preț vândut
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT m.IDMasina, md.Denumire AS Model, ma.Denumire AS Marca, v.Pret_final
            FROM Vanzare v
            INNER JOIN Masina m ON v.IDMasina = m.IDMasina
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            WHERE v.Pret_final = (SELECT MAX(CAST(Pret_final AS DECIMAL(18,2))) FROM Vanzare)";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        var masiniScumpe = new List<dynamic>();
        while (reader.Read())
        {
            string pretString = reader.GetValue(3)?.ToString();
            decimal pret = decimal.TryParse(pretString, out var tempPret) ? tempPret : 0;

            masiniScumpe.Add(new
            {
                IDMasina = reader.GetInt32(0),
                Model = reader.GetString(1),
                Marca = reader.GetString(2),
                Pret = pret
            });
        }
        ViewBag.MasiniScumpe = masiniScumpe;
        conn.Close();
    }

    return View();
}




   [HttpGet]
public IActionResult MaiMulteDate(int? kilometraj)
{
    kilometraj ??= 100000; // Valoare implicită dacă nu se introduce kilometraj

    // Datele pentru "Mașinile cele mai scumpe pentru fiecare marcă"
    var masiniScumpe = new List<dynamic>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT 
                ma.Denumire AS Marca, 
                md.Denumire AS Model, 
                m.Pret
            FROM Masina m
            INNER JOIN Model md ON m.IDModel = md.IDModel
            INNER JOIN Marca ma ON md.IDMarca = ma.IDMarca
            WHERE m.Pret = (
                SELECT MAX(mm.Pret)
                FROM Masina mm
                INNER JOIN Model mmd ON mm.IDModel = mmd.IDModel
                WHERE mmd.IDMarca = md.IDMarca
            )";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            masiniScumpe.Add(new
            {
                Marca = reader.GetString(0),
                Model = reader.GetString(1),
                Pret = reader.GetString(2)
            });
        }
        conn.Close();
    }
    ViewBag.MasiniScumpePerBrand = masiniScumpe;

    // Datele pentru "Clienții cu cele mai multe mașini cumpărate"
    var clientiTop = new List<dynamic>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            WITH ClientiVanzari AS (
                SELECT 
                    c.IDClient, 
                    c.Nume + ' ' + c.Prenume AS Client, 
                    COUNT(v.IDVanzare) AS TotalVanzari
                FROM Client c
                INNER JOIN Vanzare v ON c.IDClient = v.IDClient
                GROUP BY c.IDClient, c.Nume, c.Prenume
            )
            SELECT Client, TotalVanzari
            FROM ClientiVanzari
            WHERE TotalVanzari = (SELECT MAX(TotalVanzari) FROM ClientiVanzari)";
        SqlCommand cmd = new SqlCommand(query, conn);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            clientiTop.Add(new
            {
                Client = reader.GetString(0),
                TotalVanzari = reader.GetInt32(1)
            });
        }
        conn.Close();
    }
    ViewBag.TopClients = clientiTop;

    // Datele pentru "Vânzările pentru mașinile care depășesc un kilometraj" 
    var vanzari = new List<dynamic>();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        string query = @"
            SELECT 
                v.IDVanzare, 
                c.Nume + ' ' + c.Prenume AS Client, 
                md.Denumire AS Model, 
                MasiniFiltrate.Kilometraj, 
                v.Pret_final
            FROM (
                SELECT IDMasina, Kilometraj
                FROM Masina
                WHERE Kilometraj > @Kilometraj
            ) AS MasiniFiltrate
            INNER JOIN Vanzare v ON MasiniFiltrate.IDMasina = v.IDMasina
            INNER JOIN Client c ON v.IDClient = c.IDClient
            INNER JOIN Model md ON MasiniFiltrate.IDMasina = md.IDModel";
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Kilometraj", kilometraj.Value);
        conn.Open();
        SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            vanzari.Add(new
            {
                IDVanzare = reader.GetInt32(0),
                Client = reader.GetString(1),
                Model = reader.GetString(2),
                Kilometraj = reader.GetInt32(3),
                PretFinal = reader.GetString(4)
            });
        }
        conn.Close();
    }
    ViewBag.VanzariKilometraj = vanzari;
    ViewBag.Kilometraj = kilometraj;

    return View("MaiMulteDate");
}






  



    }

    // Clase pentru reprezentarea datelor din baza de date
    public class Car
{
    public int IDMasina { get; set; }
    public int AnFabricatie { get; set; }
    public int Kilometraj { get; set; }
    public string Pret { get; set; }
    public string Stare { get; set; }
    public int IDModel { get; set; }  // ID-ul modelului
    public int IDMarca { get; set; }  // ID-ul mărcii
    public string Model { get; set; }  // Numele modelului
    public string Marca { get; set; }  // Numele mărcii
}


    public class Client
    {
        public int IDClient { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
    }

    public class Sale
{
    public int IDVanzare { get; set; }
    public DateTime DataVanzare { get; set; }
    public string PretFinal { get; set; }
    public int IDClient { get; set; }   // ID-ul clientului
    public int IDMasina { get; set; }   // ID-ul mașinii
    public int IDAngajat { get; set; }  // ID-ul angajatului
    public string Client { get; set; }   // Numele complet al clientului
    public string Marca { get; set; }    // Numele mărcii
    public string Model { get; set; }    // Numele modelului
    public string Angajat { get; set; }  // Numele complet al angajatului
}

    public class Employee
{
    public int IDAngajat { get; set; }
    public string Nume { get; set; }
    public string Prenume { get; set; }
}

public class Dealer
{
    public int IDDealer { get; set; }
    public string Nume { get; set; }
    public string Adresa { get; set; }
    public string Telefon { get; set; }
    public string Email { get; set; }
}

public class Brand
{
    public int IDMarca { get; set; }
    public string Denumire { get; set; }
    public string Descriere { get; set; }
}

public class Model
{
    public int IDModel { get; set; }
    public string Denumire { get; set; }
    public int IDMarca { get; set; }
}
}
