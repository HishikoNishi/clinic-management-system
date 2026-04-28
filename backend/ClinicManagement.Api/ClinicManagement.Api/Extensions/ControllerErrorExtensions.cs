using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.Api.Extensions
{
    public static class ControllerErrorExtensions
    {
        public static IActionResult ApiBadRequest(this ControllerBase controller, string message, string code = "bad_request", object? details = null)
            => controller.BadRequest(new ApiErrorResponse { Code = code, Message = message, Details = details });

        public static IActionResult ApiUnauthorized(this ControllerBase controller, string message = "Unauthorized.", string code = "unauthorized")
            => controller.Unauthorized(new ApiErrorResponse { Code = code, Message = message });

        public static IActionResult ApiNotFound(this ControllerBase controller, string message, string code = "not_found")
            => controller.NotFound(new ApiErrorResponse { Code = code, Message = message });

        public static IActionResult ApiConflict(this ControllerBase controller, string message, string code = "conflict")
            => controller.Conflict(new ApiErrorResponse { Code = code, Message = message });
    }
}
