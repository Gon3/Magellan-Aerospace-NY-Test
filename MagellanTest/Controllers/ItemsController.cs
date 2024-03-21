using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using MagellanTest.Models;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateItem(ItemModel item, [FromServices] IConfiguration config){
            var dataSource = NpgsqlDataSource.Create(config.GetConnectionString("default"));

            if(item.parent_item != null){
                var command1 = dataSource.CreateCommand("select * from item where id = @parent_item;");
                command1.Parameters.AddWithValue("parent_item", NpgsqlDbType.Integer, item.parent_item);
                var reader = command1.ExecuteReader();
                if(!reader.HasRows){
                    return BadRequest("Parent item must exist before referencing it for another item.");
                }
            } else {
                var command1 = dataSource.CreateCommand("select * from item where item_name = @item_name and parent_item is null;");
                command1.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item.item_name);
                var reader = command1.ExecuteReader();
                if(reader.HasRows){
                    return BadRequest("Item name must be unique if parent item is null.");
                }
            }
            
            var command2 = dataSource.CreateCommand("insert into item (item_name, parent_item, cost, req_date) values (@item_name, @parent_item, @cost, @req_date) returning id;");
            command2.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item.item_name);
            if(item.parent_item == null)
                command2.Parameters.AddWithValue("parent_item", DBNull.Value);
            else
                command2.Parameters.AddWithValue("parent_item", NpgsqlDbType.Integer, item.parent_item);
            command2.Parameters.AddWithValue("cost", NpgsqlDbType.Integer, item.cost);
            command2.Parameters.AddWithValue("req_date", NpgsqlDbType.Date, DateTime.ParseExact(item.req_date, "MM-dd-yyyy", null));

            var res = command2.ExecuteScalar();
            return new JsonResult(new {id = res});
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemModel> GetItem(int id, [FromServices] IConfiguration config){
            var dataSource = NpgsqlDataSource.Create(config.GetConnectionString("default"));
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
        public IActionResult getTotalCost(string item_name, [FromServices] IConfiguration config){
            var dataSource = NpgsqlDataSource.Create(config.GetConnectionString("default"));
            var command1 = dataSource.CreateCommand("select * from item where item_name = @item_name;");
            command1.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item_name);
            var reader = command1.ExecuteReader();

            if(!reader.HasRows){
                return NotFound("item_name not found");
            }

            var command2 = dataSource.CreateCommand("select get_total_cost(@item_name);");
            command2.Parameters.AddWithValue("item_name", NpgsqlDbType.Varchar, item_name);

            var res = command2.ExecuteScalar();
            return new JsonResult(new {get_total_cost = res});
        }
    }
}