FROM node:alpine
WORKDIR /home/app
COPY app .
RUN npm install -g http-server && npm install
CMD ["http-server"]


