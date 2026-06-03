# Stage 1: Build the Angular application
FROM node:20-alpine as build
WORKDIR /app

# Install dependecies first(better caching)
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve the app with nginx
FROM nginx:alpine
# COPY the custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Copy built asset from build stage
COPY --from=build /app/dist/recall-app/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]