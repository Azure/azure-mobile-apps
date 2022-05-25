// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    internal static class EntityUtils
    {
        /// <summary>
        /// Error number for unique constraint violation
        /// </summary>
        private const int SqlUniqueConstraintViolationError = 2627;

        /// <summary>
        /// Creates a description of validation errors provided by <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex">The <see cref="DbEntityValidationException"/> with the validation errors.</param>
        /// <returns>A <see cref="string"/> describing the validation errors.</returns>
        public static string GetValidationDescription(DbEntityValidationException ex)
        {
            StringBuilder message = new StringBuilder();
            if (ex != null && ex.EntityValidationErrors != null)
            {
                foreach (DbEntityValidationResult result in ex.EntityValidationErrors)
                {
                    foreach (DbValidationError error in result.ValidationErrors)
                    {
                        message.AppendLine(EFResources.DomainManager_ValidationErrorDescription.FormatForUser(error.PropertyName, error.ErrorMessage));
                    }
                }
            }

            if (message.Length == 0)
            {
                message.Append(ex.Message);
            }

            return message.ToString();
        }

        /// <summary>
        /// Submits the change through Entity Framework while logging any exceptions
        /// and produce appropriate <see cref="HttpResponseMessage"/> instances.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        public static async Task<int> SubmitChangesAsync(DbContext context, HttpRequestMessage request, Func<DbUpdateConcurrencyException, object> getOriginalValue)
        {
            HttpConfiguration config = request.GetConfiguration();
            ITraceWriter traceWriter = config.Services.GetTraceWriter();
            try
            {
                int result = await context.SaveChangesAsync();
                return result;
            }
            catch (DbEntityValidationException ex)
            {
                string validationDescription = EntityUtils.GetValidationDescription(ex);
                string validationError = EFResources.DomainManager_ValidationError.FormatForUser(validationDescription);
                traceWriter.Debug(validationError, request, LogCategories.TableControllers);
                HttpResponseMessage invalid = request.CreateErrorResponse(HttpStatusCode.BadRequest, validationError, ex);
                throw new HttpResponseException(invalid);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                string conflictError = EFResources.DomainManager_ChangeConflict.FormatForUser(ex.Message);
                traceWriter.Info(conflictError, request, LogCategories.TableControllers);

                var content = getOriginalValue != null ? getOriginalValue(ex) : conflictError;
                HttpStatusCode statusCode = GetConflictStatusCode(request);
                HttpResponseMessage conflict = request.CreateResponse(statusCode, content);
                throw new HttpResponseException(conflict);
            }
            catch (DbUpdateException ex)
            {
                HttpResponseMessage error;
                Exception baseEx = ex.GetBaseException();
                SqlException sqlException = baseEx as SqlException;
                if (sqlException != null && sqlException.Number == SqlUniqueConstraintViolationError)
                {
                    string message = CommonResources.DomainManager_Conflict.FormatForUser(sqlException.Message);
                    error = request.CreateErrorResponse(HttpStatusCode.Conflict, message);
                    traceWriter.Info(message, request, LogCategories.TableControllers);
                }
                else
                {
                    string message = EFResources.DomainManager_InvalidOperation.FormatForUser(baseEx.Message);
                    error = request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                    traceWriter.Error(message, request, LogCategories.TableControllers);
                }

                throw new HttpResponseException(error);
            }
        }

        /// <summary>
        /// If the inner exception chain of the specified <see cref="DataException"/> has
        /// a <see cref="SqlException"/> that inner exception is returned.
        /// </summary>
        /// <param name="dataException">The exception to search through</param>
        /// <returns>The inner <see cref="SqlException"/> or null</returns>
        public static SqlException GetUnderlyingSqlException(DataException dataException)
        {
            if (dataException == null)
            {
                throw new ArgumentNullException("dataException");
            }

            Exception currentException = dataException;
            while (currentException != null)
            {
                SqlException sqlException = currentException as SqlException;
                if (sqlException != null)
                {
                    return sqlException;
                }

                currentException = currentException.InnerException;
            }

            return null;
        }

        /// <summary>
        /// Returns a conflict HTTP status code depending on whether there is an <c>If-Match</c> header present or not.
        /// In this case, the status code is 412 (Precondition failed), if not it is 409 (Conflict).
        /// </summary>
        /// <param name="request">The request to inspect.</param>
        /// <returns>The <see cref="HttpStatusCode"/> status.</returns>
        public static HttpStatusCode GetConflictStatusCode(HttpRequestMessage request)
        {
            return request.Headers.IfMatch.Count > 0 ? HttpStatusCode.PreconditionFailed : HttpStatusCode.Conflict;
        }

        /// <summary>
        /// Returns the SQL command with the given <paramref name="name"/> by reading the corresponding
        /// embedded resource.
        /// </summary>
        /// <param name="name">The name of the SQL command to read as an embedded resource.</param>
        /// <returns>The command.</returns>
        public static string GetSqlCommand(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            string qualifiedPath = "{0}.Sql.{1}".FormatInvariant(typeof(EntityContext).Namespace, name);
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(qualifiedPath)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}