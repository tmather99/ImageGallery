FROM mcr.microsoft.com/mssql/server
EXPOSE 1443

ARG PROJECT_DIR=/tmp/globomantics
RUN mkdir -p $PROJECT_DIR
WORKDIR $PROJECT_DIR
COPY mssql/InitializeGlobomanticsDb.sql .
COPY mssql/wait-for-it.sh .
COPY mssql/entrypoint.sh .
COPY mssql/setup.sh .

CMD ["/bin/bash", "entrypoint.sh"]