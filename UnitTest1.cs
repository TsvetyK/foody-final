using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;





namespace Foody
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string createdFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]

        public void Setup()
        {
            string token = GetJwtTolken("Tsvety", "123tsvety12"); //to add my creds

            var option = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(option);
        }


        

        private string GetJwtTolken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);
             
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

           
        }

        //Test 

        [Test, Order(1)]
        public void CreatedFood_ShouldReturnCreated()
        {
            var food = new
            {
                Name = "New Food",
                Description = "Delicious new food",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var responce = client.Execute(request);

            Assert.That(responce.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(responce.Content);

            createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty");
    
        }

        [Test, Order(2)]
        public void EditFoodTile_ShouldReturnOK()
        {
            var changes = new[]
            {
                new {path = "/name", op = "replace", value = "Updated food name"}
            };
            
            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);
             
            request.AddJsonBody(changes);

            var response = client.Execute(request);
            

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Successfully edited"));



        }

        [Test, Order(3)]

        public void GetAllFoods_ShouldReturnList ()
        {
            var request = new RestRequest("/api/food/All", Method.Get);
            
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(((HttpStatusCode)HttpStatusCode.OK)));

            var foods = JsonSerializer.Deserialize<List<object>> (response.Content);

            Assert.That(foods, Is.Not.Empty);
        }
        [Test, Order(4)]
        public void DeleteFood_ShouldReturnOK()

        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
            var response = client.Execute(request);     
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }


        [Test,Order(5)]

        public void CreateFood_WithoutRequiredFilds_ShouldReturnBadRequest()
        {
            var food = new
            {
                name = "",
                descriptionn = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);

            request.AddJsonBody(food);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.BadRequest));

        }

        [Test,Order(6)]

        public void EditingNonExistingFood_ShouldReturnNotFound()
        {
            string fakeId = "123";
            var changes = new[]
            {
                new { path = "/name", op = "replace", value = "New Title"}
            };
            var request = new RestRequest($"/Food/api/Edit/{fakeId}", Method.Patch);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            //Assert.That(response.Content, Does.Contain("No food revues..."));
        }
        [Test,Order (7)]
        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeId = "123";

            var request = new RestRequest($"/api/Food/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));


        }





        [OneTimeTearDown]
        public void Cleanup () 
        {
          client?.Dispose();
        }
    }
}