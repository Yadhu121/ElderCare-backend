using Microsoft.AspNetCore.Mvc;
using wellcare.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;

namespace wellcare.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/elder/prescription")]
    public class PrescriptionController : ControllerBase
    {
        private readonly PrescriptionTable _prescriptionTable;

        public PrescriptionController(PrescriptionTable prescriptionTable)
        {
            _prescriptionTable = prescriptionTable;
        }

        [HttpGet("{elderId}")]
        public IActionResult GetPrescriptions(int elderId)
        {
            var prescriptions = _prescriptionTable.GetPrescriptionsByElderId(elderId);
            return Ok(prescriptions);
        }

        [HttpPost]
        public IActionResult AddPrescription([FromBody] Prescription model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim != null)
            {
                model.CaretakerID = int.Parse(caretakerIdClaim);
            }

            int id = _prescriptionTable.AddPrescription(model);
            if (id > 0)
            {
                model.PrescriptionID = id;
                return CreatedAtAction(nameof(GetPrescriptions), new { elderId = model.ElderID }, model);
            }

            return StatusCode(500, "A problem occurred while handling your request.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePrescription(int id, [FromBody] Prescription model)
        {
            if (id != model.PrescriptionID)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool success = _prescriptionTable.UpdatePrescription(model);
            if (success)
                return Ok("Updated successfully.");

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePrescription(int id)
        {
            bool success = _prescriptionTable.DeletePrescription(id);
            if (success)
                return Ok("Deleted successfully.");

            return NotFound();
        }
    }
}
