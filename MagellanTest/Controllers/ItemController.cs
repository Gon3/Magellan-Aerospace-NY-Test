using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using MagellanTest.Models;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private const string CONN_STRING = "Host=localhost;Username=postgres;Password=mojoe1465;Database=part";

        [HttpPost]
        public JsonResult CreateItem(ItemModel item){
            var dataSource = NpgsqlDataSource.Create(CONN_STRING);
            var command = dataSource.CreateCommand("insert into item (item_name, parent_item, cost, req_date) values (@item_name, @parent_item, @cost, @req_date) returning id;");
            command.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item.item_name);
            if(item.parent_item == null)
                command.Parameters.AddWithValue("parent_item", DBNull.Value);
            else
                command.Parameters.AddWithValue("parent_item", NpgsqlDbType.Integer, item.parent_item);
            command.Parameters.AddWithValue("cost", NpgsqlDbType.Integer, item.cost);
            command.Parameters.AddWithValue("req_date", NpgsqlDbType.Date, DateTime.ParseExact(item.req_date, "MM-dd-yyyy", null));

            var res = command.ExecuteScalar();
            return new JsonResult(new {id = res});
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemModel> GetItem(int id){
            var dataSource = NpgsqlDataSource.Create(CONN_STRING);
            var command = dataSource.CreateCommand("select * from item where id = @id;");
            command.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

            var reader = command.ExecuteReader();
            var item = new ItemModel();
            while(reader.Read()){
                item.id = Convert.ToInt32(reader["id"]);
                item.item_name = reader["item_name"].ToString();
                item.parent_item = Convert.IsDBNull(reader["parent_item"]) ? null : Convert.ToInt32(reader["parent_item"]);
                item.cost = Convert.ToInt32(reader["cost"]);
                item.req_date = DateTime.Parse(reader["req_date"].ToString()).ToString("MM-dd-yyyy");
            }

            return item; 
        }

        [HttpGet("{item_name}")]
        public JsonResult getTotalCost(string item_name){
            var dataSource = NpgsqlDataSource.Create(CONN_STRING);
            var command = dataSource.CreateCommand("select get_total_cost(@item_name);");
            command.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item_name);

            var res = command.ExecuteScalar();
            return new JsonResult(new {get_total_cost = res});
        }
    }
}
