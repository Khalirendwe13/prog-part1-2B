ection A --- Entity Relationship Diagram (ERD)

A.1 Purpose

The ERD defines the relational database structure for RaceDay, including
entities, attributes, primary keys, foreign keys, constraints and
cardinality. The ERD contains eight entities:

Roles

Users

Organisers

Participants

Events

Categories

Enrolments

Results

The ERD should be stored in /docs/RaceDay_ERD.png.

A.2 Entities

Roles

Stores the roles available in the system.

Attribute     Description

RoleID        Primary key
RoleName      Unique role name, such as Organiser or Participant
Description   Description of the role

Users

Stores common account and authentication information.

Attribute      Description

UserID         Primary key
FirstName      User first name
LastName       User surname
Email          Unique login email
PasswordHash   Hashed password; never plain text
RoleID         Foreign key to Roles
CreatedAt      Account creation date

Organisers

Stores organiser-specific information.

Attribute          Description

OrganiserID        Primary key
UserID             Foreign key to Users
OrganisationName   Organisation represented by organiser
Phone              Contact number

Participants

Stores participant-specific information.

Attribute       Description

ParticipantID   Primary key
UserID          Foreign key to Users
DateOfBirth     Participant date of birth
Phone           Contact number

Events

Stores race events created by organisers.

Attribute     Description

EventID       Primary key
OrganiserID   Foreign key to Organisers
EventName     Name of the event
Description   Event description
EventDate     Event date
Location      Event location
Distance      Race distance
EventType     Run, Walk or Cycle
CreatedAt     Date created

Categories

Stores categories belonging to an event.

Attribute      Description

CategoryID     Primary key
EventID        Foreign key to Events
CategoryName   Category name, such as Under 20, Senior or 10km
MinimumAge     Optional minimum age
MaximumAge     Optional maximum age
Distance       Optional category distance
CreatedAt      Date created

Enrolments

Records a participant's entry into an event and selected category.

Attribute       Description

EnrolmentID     Primary key
ParticipantID   Foreign key to Participants
EventID         Foreign key to Events
CategoryID      Foreign key to Categories
EnrolmentDate   Date of enrolment

A unique constraint on (ParticipantID, EventID) prevents the same
participant from entering the same event more than once.

Results

Stores a participant's result after an event.

Attribute           Description

ResultID            Primary key
EnrolmentID         Unique foreign key to Enrolments
FinishTime          Participant finishing time
FinishingPosition   Finishing position
RecordedAt          Date result was recorded

A.3 Relationships and Cardinality

Relationship            Cardinality             Explanation

Roles → Users           1 : Many                One role can be
assigned to many users

Users → Organisers      1 : 0..1                A user may have zero or
one organiser profile

Users → Participants    1 : 0..1                A user may have zero or
one participant profile

Organisers → Events     1 : Many                One organiser can
create many events

Events → Categories     1 : Many                One event can have many
categories

Participants →          1 : Many                One participant can
Enrolments                                      have many enrolments

Events → Enrolments     1 : Many                One event can have many
enrolments

Categories → Enrolments 1 : Many                One category can have
many enrolments

The many-to-many business relationship between Participants and Events
is resolved through Enrolments.

Section B --- RESTful API Endpoint Plan

The following endpoints form the approved plan for Part 2. The
implementation should closely match these routes and behaviours.

B.1 Authentication

HTTP Method Route                  Description      Role        Request Body                                                                            Expected
Required                                                                                            Response

POST        /api/auth/register   Registers a new  None        firstName, lastName, email, password, role, organisationName?, phone?, dateOfBirth?   201 Created;
Organiser or                                                                                                         400 Bad
Participant                                                                                                          Request; 409
account.                                                                                                             Conflict

POST        /api/auth/login      Authenticates    None        email, password                                                                       200 OK;
credentials and                                                                                                      401
creates the                                                                                                          Unauthorized
authenticated
session.

B.2 User Profile

HTTP Method Route            Description   Role        Request Body                                                    Expected
Required                                                                    Response

GET         /api/profile   Returns the   Any         None                                                            200 OK;
logged-in     logged-in                                                                   401
user's own    user                                                                        Unauthorized
profile.

B.3 Events

HTTP Method Route                Description       Role        Request Body                                                         Expected Response
Required

GET         /api/events        Returns all       None        None                                                                 200 OK
available events.

GET         /api/events/{id}   Returns one       None        None                                                                 200 OK; 404
event.                                                                                             Not Found

POST        /api/events        Creates an event  Organiser   eventName, description, eventDate, location, distance, eventType   201 Created;
owned by the                                                                                       400 Bad
logged-in                                                                                          Request; 401
organiser.                                                                                         Unauthorized;
403 Forbidden

PUT         /api/events/{id}   Updates an event  Organiser   eventName, description, eventDate, location, distance, eventType   200 OK; 403
owned by the                                                                                       Forbidden;
organiser.                                                                                         404 Not Found

B.4 Categories

HTTP Method Route                                Description       Role        Request Body                                          Expected
Required                                                          Response

GET         /api/events/{eventId}/categories   Returns           None        None                                                  200 OK;
categories for an                                                                   404 Not
event.                                                                              Found

GET         /api/categories/{id}               Returns a         None        None                                                  200 OK;
category.                                                                           404 Not
Found

POST        /api/events/{eventId}/categories   Creates a         Organiser   categoryName, minimumAge?, maximumAge?, distance?   201
category for an                                                                     Created;
organiser-owned                                                                     400 Bad
event.                                                                              Request;
403
Forbidden;
404 Not
Found

PUT         /api/categories/{id}               Updates an        Organiser   categoryName, minimumAge?, maximumAge?, distance?   200 OK;
organiser-owned                                                                     403
category.                                                                           Forbidden;
404 Not
Found

B.5 Event Enrolments

HTTP Method Route                                Description     Role Required Request Body   Expected
Response

POST        /api/events/{eventId}/enrolments   Enrols the      Participant   categoryId   201 Created;
logged-in                                    400 Bad
participant in                               Request; 404
an event using                               Not Found;
a selected                                   409 Conflict
category.

GET         /api/enrolments/me                 Returns the     Participant   None           200 OK;
logged-in                                    401
participant's                                Unauthorized
enrolments.

B.6 Results

HTTP Method Route                             Description     Role Required Request Body                                   Expected
Response

POST        /api/events/{eventId}/results   Captures a      Organiser     enrolmentId, finishTime, finishingPosition   201 Created;
participant's                                                                400 Bad
finishing time                                                               Request; 403
and position.                                                                Forbidden;
404 Not
Found

GET         /api/results/me                 Returns the     Participant   None                                           200 OK;
logged-in                                                                    401
participant's                                                                Unauthorized
own results.

B.7 Role Summary

Functionality                       Public   Participant   Organiser

Register / Login                         ✓             ✓           ✓
View events                              ✓             ✓           ✓
View categories                          ✓             ✓           ✓
Own profile                            ---             ✓           ✓
Enrol in event                         ---             ✓           ✗
Own enrolments                         ---             ✓           ✗
Create/update/delete events            ---             ✗           ✓
Create/update/delete categories        ---             ✗           ✓
View event enrolments                  ---             ✗           ✓
Capture results                        ---             ✗           ✓
View own results                       ---             ✓           ✗

Section C --- SQL Database Script

C.1 Purpose

The RaceDay database is implemented using Microsoft SQL Server. The SQL
script must match the ERD in Section A exactly.

The script should be stored in /docs/RaceDay_Database.sql and should
run successfully on a clean SQL Server instance in SQL Server Management
Studio (SSMS).

C.2 Tables

The script creates:

Roles

Users

Organisers

Participants

Events

Categories

Enrolments

Results

C.3 Required Constraints

The SQL script defines:

Primary keys for every entity.

Foreign keys for every relationship.

NOT NULL constraints where data is required.

UNIQUE constraints for values such as user email and role name.

DEFAULT constraints for appropriate date/time fields.

Validation constraints for values such as event type where
applicable.

C.4 Foreign-Key Relationships

Users.RoleID → Roles.RoleID

Organisers.UserID → Users.UserID

Participants.UserID → Users.UserID

Events.OrganiserID → Organisers.OrganiserID

Categories.EventID → Events.EventID

Enrolments.ParticipantID → Participants.ParticipantID

Enrolments.EventID → Events.EventID

Enrolments.CategoryID → Categories.CategoryID

Results.EnrolmentID → Enrolments.EnrolmentID

C.5 Required Seed Data

The SQL script must include realistic sample data containing at least:

2 Organisers

2 Participants

3 Events

Categories for each event

Sample enrolments

Sample events can represent a Run, Walk and Cycle event. Categories can
include examples such as Under 20, Senior, 10km and 21km.

C.6 SQL Testing

The script should be executed in SSMS on a clean SQL Server environment.
Verify that:

The database and tables are created without errors.

All keys and constraints are created.

Seed data is inserted successfully.

Foreign-key relationships work correctly.

Duplicate emails and duplicate event enrolments are rejected as
intended.

The resulting schema matches the ERD exactly.

Part 2 --- API Implementation

Part 2 implements the design documented above using ASP.NET Core Web
API, C#, Entity Framework Core and SQL Server.

Authentication and RBAC

Users register as either Organisers or Participants and log in using
their credentials. Passwords are hashed and stored in PasswordHash,
never as plain text. Protected API operations require authentication,
and organiser-only functionality is rejected for Participants while
participant-only functionality is rejected for Organisers.

Entity Framework Core

The API uses Entity Framework Core with the Code-First approach. The
entity model must remain consistent with the eight entities defined in
the ERD and SQL script.

Swagger

Swagger/OpenAPI is integrated so that all API endpoints can be viewed
and tested from a browser. Endpoint descriptions, request models and
expected responses should be documented clearly.

Unit Testing

Tests should demonstrate both successful and unsuccessful behaviour,
including registration, login, unauthenticated access rejection,
organiser event management, role rejection, participant enrolment and
correct persistence of enrolments.

GitHub and CI/CD Requirements

Recommended repository structure:

RaceDay/
├── docs/
│   ├── RaceDay_ERD.png
│   ├── API_Endpoint_Plan.md
│   └── RaceDay_Database.sql
├── RaceDay.Api/
├── RaceDay.Tests/
├── .github/
│   └── workflows/
│       └── validate-docs.yml
└── README.md

The repository must contain at least 20 meaningful commits made
using the student's own GitHub account. GitHub Actions should validate
that the /docs folder and required planning files exist and that the
repository can build/test as appropriate.

A successful green CI build screenshot must be added to this README
before final submission.

The unlisted YouTube video should explain the ERD decisions, endpoint
plan, SQL design and a live SQL run in SSMS. Add the final YouTube link
to this README.

Final Submission Checklist

/docs/RaceDay_ERD.png submitted.

/docs/API_Endpoint_Plan.md submitted.

/docs/RaceDay_Database.sql submitted.

ERD has at least six entities and all keys/cardinality are
shown.

ERD and SQL schema match exactly.

Endpoint plan includes all six required columns and all required
resources.

SQL includes all PK, FK, NOT NULL, UNIQUE and DEFAULT
constraints.

SQL includes 2 Organisers, 2 Participants, 3 Events, categories
for every event and sample enrolments.

SQL tested successfully in SSMS.

GitHub Actions workflow is successful.

CI green-build screenshot added to README.

At least 20 meaningful commits made.

Unlisted YouTube video recorded and linked.

Part 2 implementation closely follows this Part 1 plan.

Conclusion

The RaceDay solution uses the ERD as the database blueprint, the
endpoint plan as the API blueprint, and the SQL script as the database
implementation blueprint. Part 2 should implement these approved designs
without unexplained deviations.
