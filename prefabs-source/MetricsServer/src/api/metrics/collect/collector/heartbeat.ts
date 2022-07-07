import {statsd} from '../../../../statsd'
import {Collector} from '..'

export const collectHeartbeat: Collector = (tags) => {
  const fps = Number.parseInt(tags.fps, 10)

  if (Number.isNaN(fps)) statsd.increment('Heartbeat', tags)
  else statsd.gauge('Heartbeat.fps', fps, tags)
}
