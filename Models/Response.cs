namespace RabbitQM.Helper.Models
{
    using RabbitMQ.Client.Helpers;

    #region Response<T>

    /// <summary>
    /// Класс формата ответа.
    /// </summary>
    /// <typeparam name="T">Тип отдаваемых данных.</typeparam>
    public class Response<T>
    {
        /// <summary>
        /// Данные.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Код ответа, если 0 - ответ успешный.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Сообщение.
        /// </summary>
        public string Message { get; set; }

        #region DoMethod(action, errorHandler)

        /// <summary>
        /// Метод для оборачивания исключений.
        /// </summary>
        /// <param name="action">Делегат-обертка.</param>
        /// <param name="errorHandler">Делегат-обертка для исключений.</param>
        /// <returns>Результат обработки метода.</returns>
        public static Response<T> DoMethod(Action<Response<T>> action, Action<Response<T>>? errorHandler)
        {
            Response<T> result = new ();
            try
            {
                action(result);
            }
            catch (ResponseException e)
            {
                string? showCustomErrorsAdditional = Environment.GetEnvironmentVariable("ShowCustomErrorsAdditional");
                GrafanaLogHelper.WriteLog($"{e.Message}", GrafanaLogLevel.ERROR);
                result.Code = e.Code;
                result.Message = e.Message;

                // Логируем StackTrace для кастомных ошибок.
                if (!string.IsNullOrEmpty(showCustomErrorsAdditional))
                {
                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        GrafanaLogHelper.WriteLog($"StackTrace: {e.StackTrace}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"InnerException: {e.InnerException?.Message}");
                    }

                    if (!string.IsNullOrEmpty(e.StackTrace) || !string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"**************** END OF LOG ****************", GrafanaLogLevel.INFO);
                    }
                }

                errorHandler?.Invoke(result);
            }
            catch (Exception e)
            {
                // Логгируем StackTrace/InnerException для необработанных ошибок.
                string? showUnhandledErrors = Environment.GetEnvironmentVariable("ShowUnhandledErrors");

                // Выбрасываем исключения необработанных ошибок наверх.
                string? throwUnhandledExceptions = Environment.GetEnvironmentVariable("ThrowUnhandledExceptions");
                throwUnhandledExceptions = "1";
                GrafanaLogHelper.WriteLog($"{e.Message}", GrafanaLogLevel.ERROR);
                result.Code = -1;
                result.Message = e.Message;

                // Логгируем ещё Unhandled exception.
                if (!string.IsNullOrEmpty(showUnhandledErrors))
                {
                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        GrafanaLogHelper.WriteLog($"StackTrace: {e.StackTrace}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"InnerException: {e.InnerException?.Message}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.StackTrace) || !string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"**************** END OF LOG ****************", GrafanaLogLevel.INFO);
                    }
                }

                errorHandler?.Invoke(result);
                if (!string.IsNullOrEmpty(throwUnhandledExceptions))
                {
                    throw new Exception(e.Message);
                }
            }

            return result;
        }
        #endregion

        #region DoMethod(action)

        /// <summary>
        /// Метод для оборачивания исключений.
        /// </summary>
        /// <param name="action">Делегат-обертка.</param>
        /// <returns>Результат обработки метода.</returns>
        public static Response<T> DoMethod(Action<Response<T>> action) =>
            DoMethod(action, null);
        #endregion

        #region DoMethodAsync(action)

        /// <summary>
        /// Метод для оборачивания исключений.
        /// </summary>
        /// <param name="action">Делегат-обертка.</param>
        /// <returns>Результат обработки метода.</returns>
        public static async Task<Response<T>> DoMethodAsync(Func<Response<T>, Task<Response<T>>> action) =>
            await DoMethodAsync(action, null);
        #endregion

        #region DoMethodAsync(action, errorHandler)

        /// <summary>
        /// Асинхронный метод для оборачивания исключений.
        /// </summary>
        /// <param name="action">Делегат-обертка.</param>
        /// <param name="errorHandler">Делегат-обертка для исключений.</param>
        /// <returns>Результат обработки метода.</returns>
        public static async Task<Response<T>> DoMethodAsync(Func<Response<T>, Task<Response<T>>> action, Func<Response<T>, Task<Response<T>>>? errorHandler)
        {
            Response<T> result = new ();
            try
            {
                await action(result);
            }
            catch (ResponseException e)
            {
                string? showCustomErrorsAdditional = Environment.GetEnvironmentVariable("ShowCustomErrorsAdditional");
                GrafanaLogHelper.WriteLog($"{e.Message}", GrafanaLogLevel.ERROR);
                result.Code = e.Code;
                result.Message = e.Message;

                // Логируем StackTrace для кастомных ошибок.
                if (!string.IsNullOrEmpty(showCustomErrorsAdditional))
                {
                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        GrafanaLogHelper.WriteLog($"StackTrace: {e.StackTrace}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"InnerException: {e.InnerException?.Message}");
                    }

                    if (!string.IsNullOrEmpty(e.StackTrace) || !string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"**************** END OF LOG ****************", GrafanaLogLevel.INFO);
                    }
                }

                errorHandler?.Invoke(result);
            }
            catch (Exception e)
            {
                // Логгируем StackTrace/InnerException для необработанных ошибок.
                string? showUnhandledErrors = Environment.GetEnvironmentVariable("ShowUnhandledErrors");

                // Выбрасываем исключения необработанных ошибок наверх.
                string? throwUnhandledExceptions = Environment.GetEnvironmentVariable("ThrowUnhandledExceptions");
                throwUnhandledExceptions = "1";
                GrafanaLogHelper.WriteLog($"{e.Message}", GrafanaLogLevel.ERROR);
                result.Code = -1;
                result.Message = e.Message;

                // Логгируем ещё Unhandled exception.
                if (!string.IsNullOrEmpty(showUnhandledErrors))
                {
                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        GrafanaLogHelper.WriteLog($"StackTrace: {e.StackTrace}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"InnerException: {e.InnerException?.Message}", GrafanaLogLevel.ERROR);
                    }

                    if (!string.IsNullOrEmpty(e.StackTrace) || !string.IsNullOrEmpty(e.InnerException?.Message))
                    {
                        GrafanaLogHelper.WriteLog($"**************** END OF LOG ****************", GrafanaLogLevel.INFO);
                    }
                }

                errorHandler?.Invoke(result);
                if (!string.IsNullOrEmpty(throwUnhandledExceptions))
                {
                    throw new Exception(e.Message);
                }
            }

            return result;
        }
        #endregion

        #region Throw

        /// <summary>
        /// Если нужно выкинуть ошибку с определенным кодом и сообщением.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Текст ошибки.</param>
        /// <exception cref="ResponseException">Специальный Exception.</exception>
        public void Throw(int code, string message) =>
            throw new ResponseException(code, message);

        /// <summary>
        /// Если нужно выкинуть ошибку с определенным сообщением (Code = -1).
        /// </summary>
        /// <param name="message">Текст ошибки.</param>
        /// <exception cref="ResponseException">Специальный Exception.</exception>
        public void Throw(string message) =>
            throw new ResponseException(-1, message);

        /// <summary>
        /// Если нужно выкинуть ошибку с определенным кодом и сообщением при определенном условии.
        /// </summary>
        /// <param name="condition">При true - выдаст ошибку.</param>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Текст ошибки.</param>
        /// <exception cref="ResponseException">Специальный Exception.</exception>
        public void ThrowIf(bool condition, int code, string message)
        {
            if (condition)
            {
                Throw(code, message);
            }
        }

        /// <summary>
        /// Проверка на null.
        /// </summary>
        /// <param name="obj">Проверяемый объект.</param>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Текст ошибки.</param>
        public void ThrowIfNull(object obj, int code, string message) =>
            ThrowIf(obj == null, code, message);

        /// <summary>
        /// Проверка на null или пустоту строки (String.IsNullOrEmpty).
        /// </summary>
        /// <param name="str">Проверяемая строка.</param>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Текст ошибки.</param>
        public void ThrowIfEmptyString(string str, int code, string message) =>
            ThrowIf(string.IsNullOrEmpty(str), code, message);

        /// <summary>
        /// Проверка на null или пустоту массива.
        /// </summary>
        /// <param name="array">Проверяемая строка.</param>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Текст ошибки.</param>
        public void ThrowIfEmptyArray(IEnumerable<object> array, int code, string message) =>
            ThrowIf(array == null || !array.Any(), code, message);

        #endregion

        #region GetResultIfNotError()

        /// <summary>
        /// Возвращает результат если Code == 0.
        /// </summary>
        /// <returns>Data.</returns>
        /// <exception cref="ResponseException">Исходная ошибка.</exception>
        public T GetResultIfNotError() =>
            GetResultIfNotError(string.Empty);
        #endregion

        #region GetResultIfNotError(errorMessage)

        /// <summary>
        /// Возвращает результат если Code == 0.
        /// </summary>
        /// <param name="errorMessage">Текст ошибки, который добавится в начало сообщения.</param>
        /// <returns>Data.</returns>
        /// <exception cref="ResponseException">Исходная ошибка.</exception>
        public T GetResultIfNotError(string errorMessage)
        {
            if (Code != 0)
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    Throw(Code, Message);
                }

                Throw(Code, $"{errorMessage.Trim()} {Message}");
            }

            return Data;
        }
        #endregion

        #region GetResultIfNotError(action)

        /// <summary>
        /// Возвращает результат если Code == 0.
        /// </summary>
        /// <param name="action">Метод, который будет вызван в влуче , если code не равен 0.</param>
        /// <returns>Data.</returns>
        /// <exception cref="ResponseException">Исходная ошибка.</exception>
        public T GetResultIfNotError(Action<Response<T>> action) =>
            GetResultIfNotError(string.Empty, action);
        #endregion

        #region GetResultIfNotError(errorMessage, action)

        /// <summary>
        /// Возвращает результат если Code == 0.
        /// </summary>
        /// <param name="errorMessage">Текст ошибки, который добавится в начало сообщения.</param>
        /// <param name="action">Метод, который будет вызван в случае, если code не равен 0.</param>
        /// <returns>Data.</returns>
        public T GetResultIfNotError(string errorMessage, Action<Response<T>> action)
        {
            if (Code != 0)
            {
                action?.Invoke(this);

                if (string.IsNullOrEmpty(errorMessage))
                {
                    Throw(Code, Message);
                }

                Throw(Code, $"{errorMessage.Trim()} {Message}");
            }

            return Data;
        }
        #endregion
    }
    #endregion

    #region ResponseException
    public class ResponseException : Exception
    {
        public int Code { get; set; } = -1;

        public ResponseException(string message)
            : base(message)
        {
        }

        public ResponseException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
    #endregion
}