# Stage 1: Compile and Build angular codebase

# In case we don't want root user to have authority of manipulating app files we can use another user
# FROM node:latest
# RUN mkdir /home/node/app && chown node:node /home/node/app
# RUN mkdir /home/node/app/node_modules && chown node:node /home/node/app/node_modules
# WORKDIR  /home/node/app
# USER node
# COPY --chown=node:node package.json package-lock.json ./
# RUN npm ci --quiet
# COPY --chown=node:node . .

# Use official node image as the base image
FROM node:latest as build

# Set the working directory
WORKDIR /app

# ENV PATH /app/node_modules/.bin:$PATH

# Copy package.json file
COPY /Clients/Shopping.Angular/package.json /Clients/Shopping.Angular/package-lock.json ./

# Install Angular CLI to run Build
RUN npm install -g @angular/cli

# Install all the dependencies
RUN npm install

# Copy content of application root folder to container path
COPY /Clients/Shopping.Angular .

# Build app
RUN npm run build --  --output-path=dist/shopping.angular

# Stage 2: Serve app with nginx server

# Whenever Docker comes across a second FROM instruction, it treats it as a new “build step” meaning that the container instructions have been completed.
# BASE IMAGE
FROM nginx:alpine as runtime

# Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

# Remove default nignx configuration
#RUN rm /etc/nginx/conf.d/default.conf

# Copy dist folder from build stage to nginx public folder
# onto the specified path in the current container
# According to the nginx documentation, in order for a file to be hosted in the nginx webserver we would need to paste the contents under “/usr/share/nginx/html” path
COPY --from=build /app/dist/shopping.angular /usr/share/nginx/html

# Use this if you don't use docker compose to run container
# -------------------------------------
#COPY app/certs /etc/ssl/private
#COPY ./Clients/Shopping.Angular/certs/* /etc/ssl/private
#COPY ./Clients/Shopping.Angular/certs/angularclient.crt /etc/ssl/private/angularclient.crt
#ADD /Clients/Shopping.Angular/certs /etc/ssl/private

# Copy nginx config file (if you don't use default one)
#COPY ./Clients/Shopping.Angular/nginx/nginx.conf /etc/nginx/nginx.conf
#COPY /Clients/Shopping.Angular/nginx/nginx.conf /etc/nginx/conf.d/default.conf # replace the default configuration if we don't delete it previously
# ------------------------------------

EXPOSE 80
EXPOSE 443
EXPOSE 8200

# when the container starts up, it replaces the environment variable names in the template file, 
# with the values of those variables, and overwrites the settings.json file with the result, before starting the webserver
CMD ["/bin/sh",  "-c",  "envsubst < /usr/share/nginx/html/assets/settings.template.json > /usr/share/nginx/html/assets/settings.json && exec nginx -g 'daemon off;'"]
