using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ShopInfrastructure.Controllers
{
    [Route("[controller]")]
    public class QueriesController : Controller
    {
        private readonly string _connectionString;

        public QueriesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Queries
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // Query 1: Find products that haven't been ordered yet and have quantity less than specified
        [HttpGet]
        [Route("UnorderedProductsByQuantity")]
        public async Task<IActionResult> UnorderedProductsByQuantity(int? maxQuantity)
        {
            if (!maxQuantity.HasValue)
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetUnorderedProductsByQuantity", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MaxQuantity", maxQuantity.Value);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                PdName = reader["pd_name"],
                                PdQuantity = reader["pd_quantity"],
                                PdPrice = reader["pd_price"],
                                MnName = reader["mn_name"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 2: Find manufacturers that supply products ordered by a specific user
        [HttpGet]
        [Route("ManufacturersByUserOrders")]
        public async Task<IActionResult> ManufacturersByUserOrders(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetManufacturersByUserOrders", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                MnName = reader["mn_name"],
                                Products = reader["Products"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 3: Find products in the same categories as a specified product
        [HttpGet]
        [Route("ProductsInSameCategories")]
        public async Task<IActionResult> ProductsInSameCategories(int? productId)
        {
            if (!productId.HasValue)
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetProductsInSameCategories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId.Value);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                PdName = reader["pd_name"],
                                Categories = reader["Categories"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 4: Find users who haven't ordered any products that a specific user has ordered
        [HttpGet]
        [Route("UsersWithDifferentOrders")]
        public async Task<IActionResult> UsersWithDifferentOrders(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetUsersWithDifferentOrders", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                UsName = reader["UsName"],
                                OrderCount = reader["OrderCount"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 5: Find products with price higher than average in their category
        [HttpGet]
        [Route("ProductsAboveCategoryAverage")]
        public async Task<IActionResult> ProductsAboveCategoryAverage()
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetProductsAboveCategoryAverage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                PdName = reader["pd_name"],
                                PdPrice = reader["pd_price"],
                                CgName = reader["cg_name"],
                                AvgPrice = reader["AvgPrice"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 6: Find manufacturers that supply all products in a specific category
        [HttpGet]
        [Route("ManufacturersWithAllCategoryProducts")]
        public async Task<IActionResult> ManufacturersWithAllCategoryProducts(int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetManufacturersWithAllCategoryProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", categoryId.Value);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                MnName = reader["mn_name"],
                                MnCountry = reader["MnCountry"],
                                ProductCount = reader["ProductCount"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 7: Find pairs of users who ordered exactly the same products
        [HttpGet]
        [Route("UsersWithSameOrders")]
        public async Task<IActionResult> UsersWithSameOrders()
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetUsersWithSameOrders", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                User1 = reader["User1"],
                                User2 = reader["User2"],
                                CommonProducts = reader["CommonProducts"],
                                CommonProductCount = reader["CommonProductCount"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 8: Find products that share ALL categories with a specified product
        [HttpGet]
        [Route("ProductsWithAllSameCategories")]
        public async Task<IActionResult> ProductsWithAllSameCategories(int? productId)
        {
            if (!productId.HasValue)
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetProductsWithAllSameCategories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId.Value);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                PdName = reader["pd_name"],
                                Categories = reader["Categories"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 9: Find products ordered by all users in a specific shipping country
        [HttpGet]
        [Route("ProductsOrderedByAllUsersInCountry")]
        public async Task<IActionResult> ProductsOrderedByAllUsersInCountry(int? countryId)
        {
            // Get list of countries for the dropdown
            var countries = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("SELECT co_id as Id, co_name as Name FROM countries", connection))
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            countries.Add(new
                            {
                                Id = reader["Id"],
                                Name = reader["Name"]
                            });
                        }
                    }
                }
            }
            ViewBag.Countries = countries;

            if (!countryId.HasValue)
            {
                return View(new List<dynamic>());
            }

            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetProductsOrderedByAllUsersInCountry", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CountryId", countryId.Value);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                ProductName = reader["ProductName"],
                                ManufacturerName = reader["ManufacturerName"],
                                UserCount = reader["UserCount"],
                                TotalUsersInCountry = reader["TotalUsersInCountry"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 10: Find manufacturers that supply products in all categories
        [HttpGet]
        [Route("ManufacturersWithAllCategories")]
        public async Task<IActionResult> ManufacturersWithAllCategories()
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetManufacturersWithAllCategories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                ManufacturerName = reader["ManufacturerName"],
                                Country = reader["Country"],
                                CategoriesSupplied = reader["CategoriesSupplied"],
                                TotalCategories = reader["TotalCategories"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        // Query 11: Find users who ordered products from all manufacturers
        [HttpGet]
        [Route("UsersWithAllManufacturers")]
        public async Task<IActionResult> UsersWithAllManufacturers()
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetUsersWithAllManufacturers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                UserName = reader["UserName"],
                                ManufacturersOrderedFrom = reader["ManufacturersOrderedFrom"],
                                TotalManufacturers = reader["TotalManufacturers"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }

        [HttpGet]
        [Route("UserManual")]
        public IActionResult UserManual()
        {
            return View();
        }

        // Query 12: Find manufacturers with customers sharing the same birth months
        [HttpGet]
        [Route("ManufacturersWithSameCustomerBirthMonths")]
        public async Task<IActionResult> ManufacturersWithSameCustomerBirthMonths()
        {
            var results = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetManufacturersWithSameCustomerBirthMonths", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Manufacturer1 = reader["Manufacturer1"],
                                Manufacturer2 = reader["Manufacturer2"],
                                CommonBirthMonths = reader["CommonBirthMonths"],
                                MonthCount = reader["MonthCount"]
                            });
                        }
                    }
                }
            }
            return View(results);
        }
    }
}