# -----------------------------
# BUILD ANGULAR
# -----------------------------
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build


# -----------------------------
# NGINX
# -----------------------------
FROM nginx:alpine AS final

COPY --from=build /app/dist/mock-interview-platform/browser /usr/share/nginx/html

COPY nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
