CAD–ERP Integration Using Solid Edge Add-ins
A Project for 2F-Leuchten GmbH
Developed at FH CAMPUS 02 Graz
Overview

This project demonstrates how CAD and ERP systems can be seamlessly integrated using a custom Solid Edge Add-in. The implementation leverages the Solid Edge API to extract relevant part information directly from CAD models and transfer it into an ERP system in a structured and automated manner.
The goal is to streamline the data flow between engineering and production, reduce manual input, minimize errors, and improve the overall efficiency of product documentation.

Project Context

The project was developed for 2F-Leuchten GmbH, a lighting manufacturer based in Austria, as part of an academic work at FH CAMPUS 02 Graz. The integration solution focuses on supporting engineering workflows by automating the generation and transfer of part descriptions, metadata, and manufacturing-relevant attributes.

Key Features

Solid Edge Add-in Development

Implementation based on the Solid Edge API

Extraction of geometrical and material-related data from CAD models

Automated creation of part descriptions (e.g., material, dimensions, hole summaries)

Data Preparation for ERP Systems

Transformation of CAD model data into ERP-compatible formats

Automatic filling of fields such as Description, Material, Thickness, and Remarks

Extensible Code Architecture

Separation of concerns (model handling, description building, ERP mapping)

Easily adaptable to different ERP systems or additional data fields

Testing & Quality Assurance

Unit tests written using xUnit and FluentAssertions

Clean and readable test structure for validating core logic

Ensures reliability of the description-building process

Technologies Used

C# / .NET Framework

Solid Edge API (COM-based integration)

xUnit (Unit testing framework)

FluentAssertions (Improves test readability and diagnostics)

