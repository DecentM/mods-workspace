import StatsD from 'hot-shots'

import {config} from './config'

export const statsd = new StatsD(config.statsd)
