// 01-init-db.js

// Read env var, with clear fallback
const envName = process.env.MONGO_INITDB_DATABASE;
const dbName = envName || "xeroeffort";

print(">>> Init script starting");
print(`>>> MONGO_INITDB_DATABASE env = '${envName}'`);
print(`>>> Using dbName = '${dbName}'`);

db = db.getSiblingDB(dbName);

const appUser = process.env.MONGO_INITDB_ROOT_USERNAME || "user";
const appPass = process.env.MONGO_INITDB_ROOT_PASSWORD || "super_secret_password";

print(`>>> Creating user '${appUser}' on db '${dbName}'`);

db.createUser({
  user: appUser,
  pwd: appPass,
  roles: [
    { role: "readWrite", db: dbName }
  ]
});

print(">>> Creating collection 'order_drafts'");
db.createCollection("order_drafts");

print(">>> Creating indexes on 'order_drafts'");

// Printful draft / external id
db.order_drafts.createIndex(
  { draftExternalId: 1 },
  { unique: true, name: "draftExternalId_unique" }
);

// Stripe PI id
db.order_drafts.createIndex(
  { stripePaymentIntentId: 1 },
  { name: "stripe_pi_idx" }
);

// Customer email for history / lookups
db.order_drafts.createIndex(
  { customerEmail: 1, createdAt: -1 },
  { name: "customer_email_created_idx" }
);

// CreatedAt for dashboards / admin listing
db.order_drafts.createIndex(
  { createdAt: -1 },
  { name: "createdAt_desc_idx" }
);

print(">>> Init script COMPLETE");
