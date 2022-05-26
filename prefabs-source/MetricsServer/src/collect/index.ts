import StatsD from 'hot-shots'
import hasha from 'hasha'

import {config} from '../config'

import {trackEvent} from './ga'

const statsd = new StatsD(config.statsd)

export const collect = async (ip: string, name: string, tags: Record<string, string>) => {
  if (config.statsd.enabled) {
    statsd.increment(name, tags)
  }

  if (config.ga.enabled) {
    const userid = hasha(ip, {algorithm: 'sha256'})

    const tracks = Object.entries(tags).map(([key, value]) => {
      return trackEvent(userid, 'com.decentm.vrchat.metrics.event', name, key, value)
    })

    await Promise.all(tracks)
  }
}
