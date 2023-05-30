# NotionPublisher
Get the necessary information and create a new page in Notion DB.
  
## Development environments
* IDE: Microsoft Visual Studio Community 2022 (64-bit) ver. 17.5.4
* SDK: .Net 6.0.100 LTS
* Docker: Docker Desktop for Windows v4.19.0
* Notion: NotionAPI version '2022-06-28'
* Redmine: v4.2.10
* PostgreSQL: 15.2-1.pgdg110+1  

## Projects

### RedmineApi
Central management API. A project that stores information that requires various services and performs DB work.

### RedmineLoader
Import issue information from Redmine and save issue information to DB through RedmineApi.

### RedminePublisher
Import updated issues through RedmineApi and create NotionPage information. After that, NotionAPI is called to create or update the page.
