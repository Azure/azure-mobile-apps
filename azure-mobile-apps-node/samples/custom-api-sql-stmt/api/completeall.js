var api = {
    // an example of executing a SQL statement directly
    get: (request, response, next) => {
        var query = {
            sql: 'UPDATE TodoItem SET complete = @completed',
            parameters: [
                { name: 'completed', value: request.query.completed }
            ]
        };

        request.azureMobile.data.execute(query)
            .then(function (results) {
                response.json(results);
            });
    },
    // an example of executing a stored procedure
    post: (request, response, next) => {
        var query = {
            sql: 'EXEC completeAllStoredProcedure @completed',
            parameters: [
                { name: 'completed', value: request.query.completed }
            ]
        };

        request.azureMobile.data.execute(query)
            .then(function (results) {
                response.json(results);
            });
    }
};

module.exports = api;
