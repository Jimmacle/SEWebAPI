#!/bin/bash

urls=`cat gridUrls.conf | egrep -v '^\#' | egrep -v '^\s*$'`

for i in ${urls}; do

  # get all door IDs
  doorResults=`curl -s ${i}/blocks?search=door | grep \"id\": | cut -d\: -f2 | awk -F',' '{print $1}'`
  # doorResults=`curl ${i}/blocks?search=door 2>/dev/null`
  # doorIDJson=`echo ${doorResults} | grep \"id\":`
  # echo "got: $doorIDJson"
  # doorJustId=`echo ${doorIDJson} | cut -d\: -f2`
  # doorIDs=`echo ${doorJustId} | awk -F',' '{print $1}'`

  for j in ${doorResults}; do
    echo ${j}
    ## TODO: create openDoor.json file
    ## TODO: curl -X PUT -d @openDoor.json $i/blocks/ --header "Content-Type:application/json"
  done
  ## TODO: cleanup after yourself

done
