import {statsd} from '../../../../statsd'
import {Collector} from '..'

export const collectInstance: Collector = (tags) => {
  const playerCount = Number.parseInt(tags.playerCount, 10)

  if (Number.isNaN(playerCount)) statsd.increment('Instance', tags)
  else statsd.gauge('Heartbeat.playerCount', playerCount, tags)
}
