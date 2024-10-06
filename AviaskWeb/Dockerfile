FROM node:lts-alpine
WORKDIR /app

RUN npm i -g http-server

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

EXPOSE 5173
ENV NODE_OPTIONS=--max-old-space-size=2048
CMD ["http-server", "dist", "-P http://127.0.0.1:5173?", "-p 5173"]