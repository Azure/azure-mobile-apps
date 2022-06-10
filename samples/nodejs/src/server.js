const express = require('express')(),
      zumo = require('azure-mobile-apps')();

zumo.tables.add('TodoItem');
express.use(zumo);
express.listen(process.env.PORT || 3000);
