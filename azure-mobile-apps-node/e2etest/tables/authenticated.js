var table = module.exports = require('azure-mobile-apps').table();

table.authorize = true;
table.softDelete = true;

table.read(function (context) {
	context.query = context.query.where({userId: context.user.id});
	return context.execute().then(function (items) {
		return context.user.getIdentity().then(function (identities) {
			identities = extractClientTokens(identities);
			identities = JSON.stringify(identities);
			items.forEach(function (item) {
				item.identities = identities;
			});
			return items;			
		});
	});
});

table.insert(function (context) {
	context.item.userId = context.user.id;
	return context.execute();
})

table.update(function (context) {
	return context.execute().then(matchUserId(context.user.id));
});

table.delete(function (context) {
	return context.execute().then(matchUserId(context.user.id));
});

function matchUserId(id) {
	return function (item) {
		if (item.userId != id) {
			var e = new Error('Mismatching user id');
			e.badRequest = true;
			throw e;
		}
		return item;
	}
}

function extractClientTokens(identities) {
	return Object.keys(identities).reduce(function (tokens, provider) {
		tokens[provider] = {
			access_token: identities[provider].access_token
		};
		if (provider === 'twitter') {
			tokens[provider].access_token_secret = identities[provider].access_token_secret;
		}
		return tokens;
	}, {});
}