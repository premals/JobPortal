# JobPortal

JobPortal – Docker Build & Push Guide

This document explains how to build and push all JobPortal microservice Docker images to Azure Container Registry (ACR) from Windows PowerShell.

📁 Repository Structure

Expected folder layout:

Job Portal
│
├── .github
├── JobPortalUi
├── RecrutingAPP
│   ├── Gateway
│   │   └── Dockerfile
│   ├── IdendityService
│   │   └── IdendityService
│   │       └── Dockerfile
│   ├── JobProviderService
│   │   └── Dockerfile
│   ├── JobSeekerService
│   │   └── Dockerfile
│   └── Shared.Contracts
│
└── README.md


⚠️ Important: All Docker commands must be run from the repo root
C:\Users\vedan\OneDrive\Desktop\Job Portal

az acr login --name acrvkpshared

Frontend
docker build -t acrvkpshared.azurecr.io/jobportal/frontend:latest -f JobPortalUi/career-connect-ui/Dockerfile JobPortalUi/career-connect-ui
docker push acrvkpshared.azurecr.io/jobportal/frontend:latest

Gateway
docker build -f RecrutingAPP/Gateway/Dockerfile -t acrvkpshared.azurecr.io/jobportal/gateway:latest RecrutingAPP ; docker push acrvkpshared.azurecr.io/jobportal/gateway:latest

Identity Service
docker build -f RecrutingAPP/IdendityService/IdendityService/Dockerfile -t acrvkpshared.azurecr.io/jobportal/identity-service:latest RecrutingAPP ; docker push acrvkpshared.azurecr.io/jobportal/identity-service:latest

Job Provider Service
docker build -f RecrutingAPP/JobProviderService/Dockerfile -t acrvkpshared.azurecr.io/jobportal/jobprovider-service:latest RecrutingAPP ; docker push acrvkpshared.azurecr.io/jobportal/jobprovider-service:latest

Job Seeker Service
docker build -f RecrutingAPP/JobSeekerService/Dockerfile -t acrvkpshared.azurecr.io/jobportal/jobseeker-service:latest RecrutingAPP ; docker push acrvkpshared.azurecr.io/jobportal/jobseeker-service:latest


📦 Resulting Images in ACR
acrvkpshared.azurecr.io/jobportal/gateway:latest
acrvkpshared.azurecr.io/jobportal/identity-service:latest
acrvkpshared.azurecr.io/jobportal/jobprovider-service:latest
acrvkpshared.azurecr.io/jobportal/jobseeker-service:latest