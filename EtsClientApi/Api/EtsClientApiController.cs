using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EtsClientCore;
using EtsWebClient.MainTimer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static EtsClientData.EtsDataContext;

namespace EtsClientApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EtsClientApiController : ControllerBase
    {
         
        private readonly ILogger<EtsClientApiController> _logger;
        private IUserServices _userServices;
        public EtsClientApiController(ILogger<EtsClientApiController> logger,IUserServices userServices)
        {
            _logger = logger;
            _userServices = userServices;
   

        }



        [HttpGet]
        public IActionResult Get()
        {
            return Ok("TheEtsClientApi");
        }

       

        //Add new user
        [HttpPost]
        [Route("User/{userId?}")]
        public async Task<IActionResult> CreateOrUpdateUser([FromBody] User user, int userId = 0)
        {
            

            if (userId == 0)
            {
                bool check = await EtsWebClient.Http.EtsHttpClient.CheckUserCredentials(user);
                if (check)
                {
                    await _userServices.CreateOrUpdateUser(user);
                    return Ok();
                }
                else
                {
                    return BadRequest("Please check the 'ETS' credentials");
                }

            }
            else
            {
                bool check = await EtsWebClient.Http.EtsHttpClient.CheckUserCredentials(user);
                if (check)
                {
                    bool update = await _userServices.CreateOrUpdateUser(user, userId);
                    if (update)
                    {
                        return Ok("The user has been updated");
                    }
                    else
                    {
                        return BadRequest($"Unable to update the user ");
                    }
                }
                else
                {
                    return BadRequest("Please check the 'ETS' credentials");
                }
              
            }


        }


        //Remove user
        [HttpPost]
        [Route("User/Delete/{EtsUserName}")]
        public async Task<IActionResult> DeleteUser(string EtsUserName)
        {
             var result = await _userServices.DeleteUser(EtsUserName);
            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound($"There is no user with '{EtsUserName}' user name");
            }
          
        }



      

        //Get data for a specific user
        [HttpGet]
        [Route("User/{EtsUserName}")]
        public async  Task<IActionResult> GetUserData(string EtsUserName)
        {
            var user = await _userServices.UserDetails(EtsUserName);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest($"The database does not contain the user '{EtsUserName}'.");
            }
          
        }








      


        //Get all reminders for a specific user
        [HttpGet]
        [Route("Reminders/{EtsUserName}")]
        public async Task<IActionResult> Reminders(string EtsUserName)
        {
            var result = await _userServices.Reminders(EtsUserName);
            if (result !=null)
            {  
                return Ok(result);

            }
            else
            {
                return BadRequest();
            }
          
        }


        //Create or update reminder

        [HttpPost]
        [Route("Reminders/{EtsUserName}/{ReminderId?}")]
        public async Task<IActionResult> CreateOrUpdateReminders([FromBody]Reminder reminder,string etsUserName,int ReminderId=0)
        {
            if (ReminderId == 0)
            {
                await _userServices.CreateOrUpdateReminder(reminder,etsUserName);
                return Ok();
            }
            else
            {
                var updateReminder = await _userServices.CreateOrUpdateReminder(reminder, etsUserName,ReminderId);
                if (updateReminder)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }

        }



        //Delete a reminder
        [HttpPost]
        [Route("Reminders/Delete/{ReminderId}")]
        public async Task<IActionResult> DeleteReminder(int reminderId)
        {
            await _userServices.DeleteReminder(reminderId);
            return Ok("Reminder deleted");
        }







        //Create or update a reminder timer
        [HttpPost]
        [Route("ReminderTimers/{ReminderId}/{ReminderTimeId?}")]
        public async Task<IActionResult> CreateOrUpdateReminderTimer([FromBody]ReminderTime reminderTime,int reminderId,int reminderTimeId = 0)
        {
            if (reminderTimeId == 0)
            {
                await _userServices.CreateOrUpdateReminderTimer(reminderTime, reminderId);
                return Ok("Reminder timer created");
            }
            else
            {
                await _userServices.CreateOrUpdateReminderTimer(reminderTime, reminderId, reminderTimeId);
                return Ok("Reminder updated");
            }
        }


        //Delete a reminder timer
        [Route("ReminderTimers/Delete/{ReminderTimeId}")]
        public async Task<IActionResult> DeleteReminderTimer(int reminderTimeId)
        {
            await _userServices.DeleteReminderTimer(reminderTimeId);
            return Ok("Reminder timer deleted");
        }




        [HttpGet]
        [Route("RemindersTimers/{ReminderID}")]
        public async Task<IActionResult> RemindersTimers(int reminderID)
        {
            var result = await _userServices.RemindersTimers(reminderID);
            return Ok(result);
        }





        
    }
}
