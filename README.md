# Centipede

Centipede is a distributed web crawler written in C# that uses redis as a message bus. Centipede recursively evaluates hyperlinks embedded
within a web page and crawls those links until it has completed a full crawl of all pages in the domain. To prevent duplication, Centipede
tracks pages that it has previously crawled.

Centipede then pushes crawl results into an Apache Solr instance where they are indexed and searchable via the Solr REST API.

Centipede is not meant for production use and will not be supported.
