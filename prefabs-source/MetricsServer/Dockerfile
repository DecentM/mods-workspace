FROM node:16.15.1-alpine3.16 as buildbase

#################
## DEPENDENCIES
#################
FROM buildbase as depskeleton

WORKDIR /app

COPY package.json yarn.lock ./

FROM depskeleton as proddeps
RUN yarn --network-concurrency 5 --network-timeout 300000 --non-interactive --no-progress --frozen-lockfile --production

FROM proddeps as devdeps
RUN yarn --network-concurrency 5 --network-timeout 300000 --non-interactive --no-progress --frozen-lockfile

#################
## BUILD
#################
FROM devdeps as build

COPY . .
RUN yarn build
COPY src/static /app/build/static

#################
## RUNTIME
#################
FROM proddeps as runtime

COPY --from=build /app/build /app/build

ENTRYPOINT [ "node", "/app/build/index.js" ]
