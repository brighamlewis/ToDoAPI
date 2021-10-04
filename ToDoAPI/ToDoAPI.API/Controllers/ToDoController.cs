using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ToDoAPI.DATA.EF;//to connect in to EF layer
using ToDoAPI.API.Models;//access to the DTO's
using System.Web.Http.Cors;

namespace ToDoAPI.API.Controllers
{
    public class ToDoController : ApiController
    {
        //In the code below, we are giving permission to consume the data this application will serve up. Browsers have a built-in block that restricts cross origin ToDo sharing and the code below informs the browser the consuming application has permission to access.
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public class ToDosController : ApiController
        {
            //Create an object that will connect to the db
            ToDoEntities db = new ToDoEntities();

            //api/ToDos/
            public IHttpActionResult GetToDos()
            {
                //Create a list of ToDos from the db
                //List<ToDoViewModel> toDoItems = db.ToDoItems.Include("Category").Select(t => new ToDoViewModel()
                List<ToDoViewModel> toDos = db.ToDoItems.Include("Category").Select(t => new ToDoViewModel()
                {
                    //Assign the parameters of the ToDos coming from the db to a Data Transfer Object
                    ToDoId = t.TodoId,
                    Action = t.Action,
                    Done = t.Done,
                    Category = new CategoryViewModel()
                    {
                        CategoryId = (int)t.CategoryId,
                        Name = t.Category.Name,
                        Description = t.Category.Description
                    }
                }).ToList<ToDoViewModel>();

                //Check on the results and handle accordingly below
                if (toDos.Count == 0)
                {
                    return NotFound();//404 error
                }

                //returns a 200 message with the list of ToDos
                return Ok(toDos);
            }//end GetToDos()

            //api/ToDos/id
            public IHttpActionResult GetToDo(int id)
            {
                ToDoViewModel toDo = db.ToDoItems.Include("Category").Where(r => r.TodoId == id).Select(r => new ToDoViewModel()
                {
                    ToDoId = r.TodoId,
                    Action = r.Action,
                    Done = r.Done,
                    Category = new CategoryViewModel()
                    {
                        CategoryId = (int)r.CategoryId,
                        Name = r.Category.Name,
                        Description = r.Category.Description
                    }
                }).FirstOrDefault();

                if (toDo == null)
                {
                    return NotFound();
                }

                return Ok(toDo);
            }//end GetToDo()

            //HttpPost - api/ToDos
            public IHttpActionResult PostToDo(ToDoViewModel todo)
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid Data");//This is a scopeless if statement. Great for returns in methods that only require one line of code.

                //When we are communicating with the db, we want to use the domain model
                ToDoItem newToDo = new ToDoItem()
                {
                    Action = todo.Action,
                    Done = todo.Done
                };

                db.ToDoItems.Add(newToDo);
                db.SaveChanges();
                return Ok(newToDo);
            }//end PostToDo()

            //api/ToDos (HTTPPut)
            public IHttpActionResult PutToDo(ToDoViewModel ToDo)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Data");
                }//end if

                ToDoItem existingToDo = db.ToDoItems.Where(r => r.TodoId == ToDo.ToDoId).FirstOrDefault();

                if (existingToDo != null)
                {
                    existingToDo.TodoId = ToDo.ToDoId;
                    existingToDo.Action = ToDo.Action;
                    existingToDo.Done = ToDo.Done;
                    db.SaveChanges();
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }//end PutToDo

            //api/ToDos (HTTPDelete)
            public IHttpActionResult DeleteToDo(int id)
            {
                ToDoItem ToDo = db.ToDoItems.Where(r => r.TodoId == id).FirstOrDefault();

                if (ToDo != null)
                {
                    db.ToDoItems.Remove(ToDo);
                    db.SaveChanges();
                    return Ok();
                }//end if
                else
                {
                    return NotFound();
                }//end else
            }//end DeleteToDo()

            //We use Dispose() below to dispose of any connections to the db after we are done with them - best practice handle performance - dispose of instance of the controller along with the open db connection when we are done with it.
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    db.Dispose();//close the db connection
                }
                base.Dispose(disposing);
            }

        }//end class
    }//end namespace
}
